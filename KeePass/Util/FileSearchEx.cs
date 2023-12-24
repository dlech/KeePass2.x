/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.App;
using KeePass.DataExchange.Formats;
using KeePass.Forms;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.Util
{
	internal static class FileSearchEx
	{
		private delegate FsxResult FsxMatchFn(string strFile);

		private sealed class FsxContext
		{
			public readonly string RootDirectory;
			public readonly string VolumeLabel;
			public readonly FsxMatchFn Match;

			public volatile bool End = false;

			private readonly List<FsxResult> m_lResults = new List<FsxResult>();
			private readonly object m_oResultsSync = new object();

			public FsxContext(string strRoot, string strVolumeLabel,
				FsxMatchFn fMatch)
			{
				if(strRoot == null) throw new ArgumentNullException("strRoot");
				if(strVolumeLabel == null) throw new ArgumentNullException("strVolumeLabel");
				if(fMatch == null) throw new ArgumentNullException("fMatch");

				this.RootDirectory = strRoot;
				this.VolumeLabel = strVolumeLabel;
				this.Match = fMatch;
			}

			public void AddResult(FsxResult r)
			{
				if(r == null) { Debug.Assert(false); return; }

				lock(m_oResultsSync) { m_lResults.Add(r); }
			}

			public List<FsxResult> GetResults()
			{
				List<FsxResult> l;
				lock(m_oResultsSync)
				{
					m_lResults.Sort(FsxResult.CompareByPath);
					l = new List<FsxResult>(m_lResults);
				}
				return l;
			}

			public static int CompareByRoot(FsxContext a, FsxContext b)
			{
				if(a == null) { Debug.Assert(false); return ((b == null) ? 0 : -1); }
				if(b == null) { Debug.Assert(false); return 1; }

				return StrUtil.CompareNaturally(a.RootDirectory, b.RootDirectory);
			}
		}

		private sealed class FsxResult
		{
			public readonly string Path;
			public readonly string Type;

			public FsxResult(string strPath, string strType)
			{
				if(strPath == null) throw new ArgumentNullException("strPath");
				if(strType == null) throw new ArgumentNullException("strType");

				this.Path = strPath;
				this.Type = strType;
			}

			public static int CompareByPath(FsxResult a, FsxResult b)
			{
				if(a == null) { Debug.Assert(false); return ((b == null) ? 0 : -1); }
				if(b == null) { Debug.Assert(false); return 1; }

				return StrUtil.CompareNaturally(a.Path, b.Path);
			}
		}

		public static void FindDatabaseFiles(MainForm mf, string strRootPath)
		{
			if(mf == null) { Debug.Assert(false); return; }
			Debug.Assert(GlobalWindowManager.TopWindow == null); // mf should be parent

			VistaTaskDialog dlg = new VistaTaskDialog();
			dlg.CommandLinks = true;
			dlg.Content = KPRes.FileSearchModes + MessageService.NewParagraph +
				KPRes.QuickSearchQ;
			dlg.DefaultButtonID = (int)DialogResult.Yes;
			dlg.WindowTitle = PwDefs.ShortProductName;
			dlg.SetIcon(VtdCustomIcon.Question);
			dlg.AddButton((int)DialogResult.Yes, KPRes.Quick, KPRes.FileSearchQuickDesc);
			dlg.AddButton((int)DialogResult.No, KPRes.Normal, KPRes.FileSearchNormalDesc);
			dlg.AddButton((int)DialogResult.Cancel, KPRes.Cancel, null);

			int dr;
			if(dlg.ShowDialog(mf)) dr = dlg.Result;
			else
				dr = (int)MessageService.Ask(KPRes.FileSearchModes + MessageService.NewParagraph +
					KPRes.Quick + ": " + KPRes.FileSearchQuickDesc + MessageService.NewParagraph +
					KPRes.Normal + ": " + KPRes.FileSearchNormalDesc + MessageService.NewParagraph +
					KPRes.QuickSearchQ, PwDefs.ShortProductName, MessageBoxButtons.YesNoCancel);

			FsxMatchFn fMatch;
			if(dr == (int)DialogResult.Yes) fMatch = MatchDatabaseFileQuick;
			else if(dr == (int)DialogResult.No) fMatch = MatchDatabaseFileContent;
			else return;

			string strFile = FindUI(mf, fMatch, strRootPath);
			if(!string.IsNullOrEmpty(strFile))
				mf.OpenDatabase(IOConnectionInfo.FromPath(strFile), null, true);
		}

		private static volatile string[] g_vQuickSuffixes = null;
		private static FsxResult MatchDatabaseFileQuick(string strFile)
		{
			if(string.IsNullOrEmpty(strFile)) { Debug.Assert(false); return null; }

			string[] vSfx = g_vQuickSuffixes;
			if(vSfx == null)
			{
				Dictionary<string, bool> d = new Dictionary<string, bool>(
					StrUtil.CaseIgnoreComparer);

				d["." + AppDefs.FileExtension.FileExt] = true;
				d["." + KeePassKdb1x.FileExt1] = true;
				d["." + KeePassKdb1x.FileExt2] = true;
				d[FileTransactionEx.StrTempSuffix] = true;
				d[FileTransactionEx.StrTxfTempSuffix] = true;

				if(d.ContainsKey(string.Empty)) { Debug.Assert(false); d.Remove(string.Empty); }

				vSfx = (new List<string>(d.Keys)).ToArray();
				g_vQuickSuffixes = vSfx;
			}

			foreach(string strSuffix in vSfx)
			{
				if(strFile.EndsWith(strSuffix, StrUtil.CaseIgnoreCmp))
					return MatchDatabaseFileContent(strFile);
			}

			return null;
		}

		private static FsxResult MatchDatabaseFileContent(string strFile)
		{
			if(string.IsNullOrEmpty(strFile)) { Debug.Assert(false); return null; }

			const ulong u2x = ((ulong)KdbxFile.FileSignature2 << 32) |
				KdbxFile.FileSignature1;
			const ulong u2p = ((ulong)KdbxFile.FileSignaturePreRelease2 << 32) |
				KdbxFile.FileSignaturePreRelease1;
			const ulong u1x = ((ulong)KdbxFile.FileSignatureOld2 << 32) |
				KdbxFile.FileSignatureOld1;

			try
			{
				using(FileStream fs = new FileStream(strFile, FileMode.Open,
					FileAccess.Read, FileShare.Read, 64))
				{
					using(BinaryReader br = new BinaryReader(fs))
					{
						byte[] pb = br.ReadBytes(8);
						if((pb != null) && (pb.Length == 8))
						{
							ulong u = MemUtil.BytesToUInt64(pb);
							if((u == u2x) || (u == u2p))
								return new FsxResult(strFile,
									AppDefs.FileExtension.FileExt.ToUpperInvariant() +
									" (" + PwDefs.ShortProductName + " 2.x)");
							if(u == u1x)
								return new FsxResult(strFile,
									KeePassKdb1x.FileExt1.ToUpperInvariant() +
									" (" + PwDefs.ShortProductName + " 1.x)");
						}
					}
				}
			}
			catch(Exception) { }

			return null;
		}

		private static string FindUI(MainForm mf, FsxMatchFn fMatch, string strRootPath)
		{
			if(mf == null) { Debug.Assert(false); return null; }
			if(fMatch == null) { Debug.Assert(false); return null; }

			Form fOptDialog;
			IStatusLogger sl = StatusUtil.CreateStatusDialog(mf, out fOptDialog,
				null, GetSearchingText(null), true, true);
			mf.UIBlockInteraction(true);

			List<FsxContext> lContexts = new List<FsxContext>();
			string strExcp = null;
			try { lContexts = Find(fMatch, sl, strRootPath); }
			catch(Exception ex) { strExcp = ex.Message; }

			bool bAborted = !sl.ContinueWork();

			mf.UIBlockInteraction(false);
			sl.EndLogging();

			if(!string.IsNullOrEmpty(strExcp))
			{
				MessageService.ShowWarning(strExcp);
				return null;
			}

			Action<ListView> fInit = delegate(ListView lv)
			{
				int w = lv.ClientSize.Width - UIUtil.GetVScrollBarWidth();
				int ws = w / 70;
				lv.Columns.Add(KPRes.File, w / 5);
				lv.Columns.Add(KPRes.Folder, (int)(((long)w * 3L) / 10L - (ws * 3)));
				lv.Columns.Add(KPRes.Size, w / 10 + ws, HorizontalAlignment.Right);
				lv.Columns.Add(KPRes.Type, w / 5 + ws);
				lv.Columns.Add(KPRes.LastModified, w / 5 + ws);
			};

			List<object> lItems = new List<object>();
			int cFiles = 0;
			foreach(FsxContext ctx in lContexts)
			{
				List<FsxResult> lResults = ctx.GetResults();
				if(lResults.Count == 0) continue;

				string strGroup = UrlUtil.EnsureTerminatingSeparator(ctx.RootDirectory, false);
				if(ctx.VolumeLabel.Length != 0)
					strGroup += " (" + ctx.VolumeLabel + ")";
				lItems.Add(new ListViewGroup(strGroup));

				foreach(FsxResult r in lResults)
				{
					try
					{
						FileInfo fi = new FileInfo(r.Path);

						ListViewItem lvi = new ListViewItem(UrlUtil.GetFileName(r.Path));
						lvi.SubItems.Add(UrlUtil.GetFileDirectory(r.Path, true, false));
						lvi.SubItems.Add(StrUtil.FormatDataSizeKB((ulong)fi.Length));
						lvi.SubItems.Add(r.Type);
						lvi.SubItems.Add(TimeUtil.ToDisplayString(
							TimeUtil.ToLocal(fi.LastWriteTimeUtc, false)));
						lvi.Tag = r.Path;

						lItems.Add(lvi);
						++cFiles;
					}
					catch(Exception) { Debug.Assert(false); }
				}
			}

			// string strSub = KPRes.ObjectsFound.Replace("{PARAM}", cFiles.ToString()) + ".";
			string strSub = StrUtil.TrimDots(KPRes.Search, true) + ": " +
				KPRes.FileExtName2 + ".";
			if(bAborted) strSub += " " + (new OperationCanceledException()).Message;

			ListViewForm dlg = new ListViewForm();
			dlg.InitEx(KPRes.SearchGroupName, strSub, null,
				Properties.Resources.B48x48_XMag, lItems, null, fInit);
			UIUtil.ShowDialogAndDestroy(dlg, mf);

			return (dlg.ResultItem as string);
		}

		private static string GetSearchingText(List<FsxContext> lToDo)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(KPRes.SearchingOp);
			sb.Append("...");

			if((lToDo != null) && (lToDo.Count != 0))
			{
				sb.Append(" (");

				for(int i = 0; i < lToDo.Count; ++i)
				{
					if(i != 0) sb.Append(", ");
					sb.Append(lToDo[i].RootDirectory);
				}

				sb.Append(").");
			}

			return sb.ToString();
		}

		private static List<FsxContext> Find(FsxMatchFn fMatch, IStatusLogger sl,
			string strRootPath)
		{
			if(sl == null) { Debug.Assert(false); sl = new NullStatusLogger(); }

			List<FsxContext> lContexts = new List<FsxContext>();

			if(!string.IsNullOrEmpty(strRootPath))
				lContexts.Add(new FsxContext(strRootPath, string.Empty, fMatch));
			else
			{
				DriveInfo[] vDrives = DriveInfo.GetDrives();
				if(vDrives == null) { Debug.Assert(false); vDrives = MemUtil.EmptyArray<DriveInfo>(); }
				foreach(DriveInfo di in vDrives)
				{
					if(di == null) { Debug.Assert(false); continue; }

					try
					{
						if(!di.IsReady) continue;

						string strRoot = di.RootDirectory.FullName;
						if(string.IsNullOrEmpty(strRoot)) { Debug.Assert(false); continue; }

						string strVolumeLabel = string.Empty;
						try { strVolumeLabel = (di.VolumeLabel ?? string.Empty); }
						catch(Exception) { Debug.Assert(false); }

						lContexts.Add(new FsxContext(strRoot, strVolumeLabel, fMatch));
					}
					catch(Exception) { Debug.Assert(false); }
				}
			}

			for(int i = lContexts.Count - 1; i >= 0; --i)
			{
				FsxContext ctx = lContexts[i];

				try
				{
					Thread th = new Thread(delegate()
					{
						try
						{
							try { FindInDirectory(ctx.RootDirectory, ctx); }
							catch(Exception) { Debug.Assert(false); }

							ctx.End = true;
						}
						catch(Exception) { Debug.Assert(false); }
					});
					th.Start();
				}
				catch(Exception) { Debug.Assert(false); lContexts.RemoveAt(i); }
			}

			lContexts.Sort(FsxContext.CompareByRoot);
			sl.SetText(GetSearchingText(lContexts), LogStatusType.Info);

			List<FsxContext> lRunning = new List<FsxContext>(lContexts);
			int msSleep = Math.Max(PwDefs.UIUpdateDelay / 4, 1);
			while(lRunning.Count != 0)
			{
				try
				{
					Thread.Sleep(msSleep);

					for(int i = lRunning.Count - 1; i >= 0; --i)
					{
						if(lRunning[i].End)
						{
							lRunning.RemoveAt(i);
							sl.SetText(GetSearchingText(lRunning), LogStatusType.Info);
						}
					}

					if(!sl.ContinueWork())
					{
						foreach(FsxContext ctx in lRunning) { ctx.End = true; }
						lRunning.Clear();
					}
				}
				catch(Exception) { Debug.Assert(false); }
			}

			return lContexts;
		}

		private static void FindInDirectory(string strPath, FsxContext ctx)
		{
			if(string.IsNullOrEmpty(strPath)) { Debug.Assert(false); return; }
			if(ctx == null) { Debug.Assert(false); return; }

			try
			{
				string[] vFiles = Directory.GetFiles(strPath, "*",
					SearchOption.TopDirectoryOnly);
				if(vFiles == null) { Debug.Assert(false); vFiles = MemUtil.EmptyArray<string>(); }

				foreach(string strFile in vFiles)
				{
					if(string.IsNullOrEmpty(strFile)) { Debug.Assert(false); continue; }
					if(ctx.End) return;

					FsxResult r = ctx.Match(strFile);
					if(r != null) ctx.AddResult(r);
				}

				string[] vDirs = Directory.GetDirectories(strPath, "*",
					SearchOption.TopDirectoryOnly);
				if(vDirs == null) { Debug.Assert(false); vDirs = MemUtil.EmptyArray<string>(); }

				foreach(string strDir in vDirs)
				{
					if(string.IsNullOrEmpty(strDir)) { Debug.Assert(false); continue; }
					if(ctx.End) return;

					string strTerm = UrlUtil.EnsureTerminatingSeparator(strDir, false);
					if(IsDirTraversal(strTerm)) { Debug.Assert(false); continue; }

					FindInDirectory(strDir, ctx);
				}
			}
			catch(UnauthorizedAccessException) { }
			catch(Exception) { Debug.Assert(false); }
		}

		private static bool IsDirTraversal(string str)
		{
			if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return false; }
			Debug.Assert(UrlUtil.EnsureTerminatingSeparator(str, false) == str);

			string strCnc = UrlUtil.ConvertSeparators(str, '/');
			if(strCnc.EndsWith("/../")) return true;
			if(strCnc.EndsWith("/./")) return true;

			return false;
		}
	}
}
