/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Windows.Forms;

using KeePass.App;
using KeePass.Forms;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.DataExchange
{
	public static class ExportUtil
	{
		public static bool Export(PwExportInfo pwExportInfo, IStatusLogger slLogger)
		{
			if(pwExportInfo == null) throw new ArgumentNullException("pwExportInfo");
			if(pwExportInfo.DataGroup == null) throw new ArgumentException();

			if(!AppPolicy.Try(AppPolicyId.Export)) return false;

			ExchangeDataForm dlg = new ExchangeDataForm();
			dlg.InitEx(true, pwExportInfo.ContextDatabase, pwExportInfo.DataGroup);
			dlg.ExportInfo = pwExportInfo;

			bool bStatusActive = false;

			try
			{
				if(dlg.ShowDialog() == DialogResult.OK)
				{
					FileFormatProvider ffp = dlg.ResultFormat;
					if(ffp == null) { Debug.Assert(false); return false; }

					IOConnectionInfo ioc = null;
					if(ffp.RequiresFile)
					{
						string[] vFiles = dlg.ResultFiles;
						if(vFiles == null) { Debug.Assert(false); return false; }
						if(vFiles.Length == 0) { Debug.Assert(false); return false; }
						Debug.Assert(vFiles.Length == 1);

						string strFile = vFiles[0];
						if(string.IsNullOrEmpty(strFile)) { Debug.Assert(false); return false; }

						ioc = IOConnectionInfo.FromPath(strFile);
					}

					if(slLogger != null)
					{
						slLogger.StartLogging(KPRes.ExportingStatusMsg, true);
						bStatusActive = true;
					}

					Application.DoEvents(); // Redraw parent window
					return Export(pwExportInfo, ffp, ioc, slLogger);
				}
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
			finally
			{
				UIUtil.DestroyForm(dlg);
				if(bStatusActive) slLogger.EndLogging();
			}

			return false;
		}

		public static bool Export(PwExportInfo pwExportInfo, string strFormatName,
			IOConnectionInfo iocOutput)
		{
			if(strFormatName == null) throw new ArgumentNullException("strFormatName");
			// iocOutput may be null

			FileFormatProvider ffp = Program.FileFormatPool.Find(strFormatName);
			if(ffp == null) return false;

			NullStatusLogger slLogger = new NullStatusLogger();
			return Export(pwExportInfo, ffp, iocOutput, slLogger);
		}

		public static bool Export(PwExportInfo pwExportInfo, FileFormatProvider fileFormat,
			IOConnectionInfo iocOutput, IStatusLogger slLogger)
		{
			if(pwExportInfo == null) throw new ArgumentNullException("pwExportInfo");
			if(pwExportInfo.DataGroup == null) throw new ArgumentException();
			if(fileFormat == null) throw new ArgumentNullException("fileFormat");

			bool bFileReq = fileFormat.RequiresFile;
			if(bFileReq && (iocOutput == null))
				throw new ArgumentNullException("iocOutput");
			if(bFileReq && (iocOutput.Path.Length == 0))
				throw new ArgumentException();

			PwDatabase pd = pwExportInfo.ContextDatabase;
			Debug.Assert(pd != null);

			if(!AppPolicy.Try(AppPolicyId.Export)) return false;
			if(!AppPolicy.Current.ExportNoKey && (pd != null))
			{
				if(!KeyUtil.ReAskKey(pd, true)) return false;
			}

			if(!fileFormat.SupportsExport) return false;
			if(!fileFormat.TryBeginExport()) return false;

			CompositeKey ckOrgMasterKey = null;
			DateTime dtOrgMasterKey = PwDefs.DtDefaultNow;

			PwGroup pgOrgData = pwExportInfo.DataGroup;
			PwGroup pgOrgRoot = ((pd != null) ? pd.RootGroup : null);
			bool bParentGroups = (pwExportInfo.ExportParentGroups && (pd != null) &&
				(pgOrgData != pgOrgRoot));

			bool bExistedAlready = true; // No deletion by default
			bool bResult = false;

			try
			{
				if(pwExportInfo.ExportMasterKeySpec && fileFormat.RequiresKey &&
					(pd != null))
				{
					KeyCreationFormResult r;
					DialogResult dr = KeyCreationForm.ShowDialog(iocOutput, true, out r);
					if((dr != DialogResult.OK) || (r == null)) return false;

					ckOrgMasterKey = pd.MasterKey;
					dtOrgMasterKey = pd.MasterKeyChanged;

					pd.MasterKey = r.CompositeKey;
					pd.MasterKeyChanged = DateTime.UtcNow;
				}

				if(bParentGroups)
				{
					PwGroup pgNew = WithParentGroups(pgOrgData, pd);
					pwExportInfo.DataGroup = pgNew;
					pd.RootGroup = pgNew;
				}

				if(bFileReq) bExistedAlready = IOConnection.FileExists(iocOutput);

				Stream s = (bFileReq ? IOConnection.OpenWrite(iocOutput) : null);
				try { bResult = fileFormat.Export(pwExportInfo, s, slLogger); }
				finally { if(s != null) s.Close(); }

				if(bFileReq && bResult)
				{
					if(pwExportInfo.ExportPostOpen)
						NativeLib.StartProcess(iocOutput.Path);
					if(pwExportInfo.ExportPostShow)
						WinUtil.ShowFileInFileManager(iocOutput.Path, true);
				}
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
			finally
			{
				if(ckOrgMasterKey != null)
				{
					pd.MasterKey = ckOrgMasterKey;
					pd.MasterKeyChanged = dtOrgMasterKey;
				}

				if(bParentGroups)
				{
					pwExportInfo.DataGroup = pgOrgData;
					pd.RootGroup = pgOrgRoot;
				}
			}

			if(bFileReq && !bResult && !bExistedAlready)
			{
				try { IOConnection.DeleteFile(iocOutput); }
				catch(Exception) { }
			}

			return bResult;
		}

		private static PwGroup WithParentGroups(PwGroup pg, PwDatabase pd)
		{
			if(pg == null) { Debug.Assert(false); return null; }
			if(pd == null) { Debug.Assert(false); return pg; }

			Dictionary<PwUuid, bool> dUuids = CollectUuids(pg);

			PwGroup pgNew = FilterCloneGroup(pd.RootGroup, dUuids);
			Debug.Assert(pgNew.GetEntriesCount(true) == pg.GetEntriesCount(true));
			return pgNew;
		}

		private static Dictionary<PwUuid, bool> CollectUuids(PwGroup pg)
		{
			Dictionary<PwUuid, bool> d = new Dictionary<PwUuid, bool>();

			Action<IStructureItem> fAdd = delegate(IStructureItem it)
			{
				if(it == null) { Debug.Assert(false); return; }

				Debug.Assert(!d.ContainsKey(it.Uuid));
				d[it.Uuid] = true;

				PwGroup pgParent = it.ParentGroup;
				while(pgParent != null)
				{
					d[pgParent.Uuid] = true;
					pgParent = pgParent.ParentGroup;
				}
			};

			GroupHandler gh = delegate(PwGroup pgCur) { fAdd(pgCur); return true; };
			EntryHandler eh = delegate(PwEntry peCur) { fAdd(peCur); return true; };

			fAdd(pg);
			pg.TraverseTree(TraversalMethod.PreOrder, gh, eh);

			return d;
		}

		private static PwGroup FilterCloneGroup(PwGroup pg, Dictionary<PwUuid, bool> dUuids)
		{
			PwGroup pgNew = new PwGroup();
			pgNew.Uuid = pg.Uuid;
			pgNew.AssignProperties(pg, false, true);
			Debug.Assert(pgNew.EqualsGroup(pg, (PwCompareOptions.IgnoreParentGroup |
				PwCompareOptions.PropertiesOnly), MemProtCmpMode.Full));

			foreach(PwEntry pe in pg.Entries)
			{
				if(dUuids.ContainsKey(pe.Uuid))
					pgNew.AddEntry(pe.CloneDeep(), true, false);
			}

			foreach(PwGroup pgSub in pg.Groups)
			{
				if(dUuids.ContainsKey(pgSub.Uuid))
					pgNew.AddGroup(FilterCloneGroup(pgSub, dUuids), true, false);
			}

			return pgNew;
		}
	}
}
