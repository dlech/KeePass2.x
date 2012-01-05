/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2012 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Collections.Specialized;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

using KeePass.App;
using KeePass.Ecas;
using KeePass.Native;
using KeePass.UI;
using KeePass.Util;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Utility;

using KeeNativeLib = KeePassLib.Native;

namespace KeePass.Util
{
	public static partial class ClipboardUtil
	{
		private static byte[] m_pbDataHash32 = null;
		private static string m_strFormat = null;
		private static bool m_bEncoded = false;

		private const string ClipboardIgnoreFormatName = "Clipboard Viewer Ignore";

		[Obsolete]
		public static bool Copy(string strToCopy, bool bIsEntryInfo,
			PwEntry peEntryInfo, PwDatabase pwReferenceSource)
		{
			return Copy(strToCopy, true, bIsEntryInfo, peEntryInfo,
				pwReferenceSource, IntPtr.Zero);
		}

		[Obsolete]
		public static bool Copy(ProtectedString psToCopy, bool bIsEntryInfo,
			PwEntry peEntryInfo, PwDatabase pwReferenceSource)
		{
			if(psToCopy == null) throw new ArgumentNullException("psToCopy");
			return Copy(psToCopy.ReadString(), true, bIsEntryInfo, peEntryInfo,
				pwReferenceSource, IntPtr.Zero);
		}

		public static bool Copy(string strToCopy, bool bSprCompile, bool bIsEntryInfo,
			PwEntry peEntryInfo, PwDatabase pwReferenceSource, IntPtr hOwner)
		{
			if(strToCopy == null) throw new ArgumentNullException("strToCopy");

			if(bIsEntryInfo && !AppPolicy.Try(AppPolicyId.CopyToClipboard))
				return false;

			string strData = (bSprCompile ? SprEngine.Compile(strToCopy,
				new SprContext(peEntryInfo, pwReferenceSource,
				SprCompileFlags.All)) : strToCopy);

			try
			{
				if(!KeeNativeLib.NativeLib.IsUnix()) // Windows
				{
					if(!OpenW(hOwner, true))
						throw new InvalidOperationException();

					bool bFailed = false;
					if(!AttachIgnoreFormatW()) bFailed = true;
					if(!SetDataW(null, strData, null)) bFailed = true;
					CloseW();

					if(bFailed) return false;
				}
				else // Managed
				{
					Clear();

					DataObject doData = CreateProtectedDataObject(strData);
					Clipboard.SetDataObject(doData);
				}
			}
			catch(Exception) { Debug.Assert(false); return false; }

			m_strFormat = null;

			byte[] pbUtf8 = StrUtil.Utf8.GetBytes(strData);
			SHA256Managed sha256 = new SHA256Managed();
			m_pbDataHash32 = sha256.ComputeHash(pbUtf8);

			RaiseCopyEvent(bIsEntryInfo, strData);

			if(peEntryInfo != null) peEntryInfo.Touch(false);

			// SprEngine.Compile might have modified the database
			Program.MainForm.UpdateUI(false, null, false, null, false, null, false);

			return true;
		}

		[Obsolete]
		public static bool Copy(byte[] pbToCopy, string strFormat, bool bIsEntryInfo)
		{
			return Copy(pbToCopy, strFormat, false, bIsEntryInfo, IntPtr.Zero);
		}

		public static bool Copy(byte[] pbToCopy, string strFormat, bool bEncode,
			bool bIsEntryInfo, IntPtr hOwner)
		{
			Debug.Assert(pbToCopy != null);
			if(pbToCopy == null) throw new ArgumentNullException("pbToCopy");

			if(bIsEntryInfo && !AppPolicy.Try(AppPolicyId.CopyToClipboard))
				return false;

			try
			{
				if(!KeeNativeLib.NativeLib.IsUnix()) // Windows
				{
					if(!OpenW(hOwner, true))
						throw new InvalidOperationException();

					uint uFormat = NativeMethods.RegisterClipboardFormat(strFormat);

					bool bFailed = false;
					if(!AttachIgnoreFormatW()) bFailed = true;

					if(!bEncode)
					{
						if(!SetDataW(uFormat, pbToCopy)) bFailed = true;
					}
					else // Encode
					{
						string strData = Convert.ToBase64String(pbToCopy,
							Base64FormattingOptions.None);
						if(!SetDataW(uFormat, strData, false)) bFailed = true;
					}

					CloseW();

					if(bFailed) return false;
				}
				else // Managed, no encoding
				{
					Clear();

					DataObject doData = CreateProtectedDataObject(strFormat, pbToCopy);
					Clipboard.SetDataObject(doData);
				}
			}
			catch(Exception) { Debug.Assert(false); return false; }

			m_strFormat = strFormat;
			m_bEncoded = bEncode;

			SHA256Managed sha256 = new SHA256Managed();
			m_pbDataHash32 = sha256.ComputeHash(pbToCopy);

			RaiseCopyEvent(bIsEntryInfo, string.Empty);

			return true;
		}

		public static byte[] GetEncodedData(string strFormat, IntPtr hOwner)
		{
			try
			{
				if(!KeeNativeLib.NativeLib.IsUnix()) // Windows
				{
					if(!OpenW(hOwner, false))
						throw new InvalidOperationException();

					string str = GetStringW(strFormat, false);
					CloseW();

					if(str == null) return null;
					if(str.Length == 0) return new byte[0];

					return Convert.FromBase64String(str);
				}
				else // Managed, no encoding
				{
					return (byte[])Clipboard.GetData(strFormat);
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		public static bool CopyAndMinimize(string strToCopy, bool bIsEntryInfo,
			Form formContext, PwEntry peEntryInfo, PwDatabase pwReferenceSource)
		{
			return CopyAndMinimize(new ProtectedString(false, strToCopy),
				bIsEntryInfo, formContext, peEntryInfo, pwReferenceSource);
		}

		public static bool CopyAndMinimize(ProtectedString psToCopy, bool bIsEntryInfo,
			Form formContext, PwEntry peEntryInfo, PwDatabase pwReferenceSource)
		{
			if(psToCopy == null) throw new ArgumentNullException("psToCopy");

			IntPtr hOwner = ((formContext != null) ? formContext.Handle : IntPtr.Zero);

			if(Copy(psToCopy.ReadString(), true, bIsEntryInfo, peEntryInfo,
				pwReferenceSource, hOwner))
			{
				if(formContext != null)
				{
					if(Program.Config.MainWindow.DropToBackAfterClipboardCopy)
						NativeMethods.LoseFocus(formContext);

					if(Program.Config.MainWindow.MinimizeAfterClipboardCopy)
						UIUtil.SetWindowState(formContext, FormWindowState.Minimized);
				}

				return true;
			}

			return false;
		}

		private static void RaiseCopyEvent(bool bIsEntryInfo, string strDesc)
		{
			if(bIsEntryInfo == false) return;

			Program.TriggerSystem.RaiseEvent(EcasEventIDs.CopiedEntryInfo, strDesc);
		}

		/// <summary>
		/// Safely clear the clipboard. The clipboard clearing method of the
		/// .NET framework sets the clipboard to an empty <c>DataObject</c> when
		/// invoking the clearing method -- this might cause incompatibilities
		/// with other applications. Therefore, the <c>Clear</c> method of
		/// <c>ClipboardUtil</c> first tries to clear the clipboard using
		/// native Windows functions (which *really* clear the clipboard).
		/// </summary>
		public static void Clear()
		{
			bool bNativeSuccess = false;
			try
			{
				if(!KeeNativeLib.NativeLib.IsUnix()) // Windows
				{
					if(OpenW(IntPtr.Zero, true)) // Clears the clipboard
					{
						CloseW();
						bNativeSuccess = true;
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }

			if(bNativeSuccess) return;

			try { Clipboard.Clear(); } // Fallback to .NET framework method
			catch(Exception) { Debug.Assert(false); }
		}

		public static void ClearIfOwner()
		{
			// If we didn't copy anything or cleared it already: do nothing
			if(m_pbDataHash32 == null) return;
			if(m_pbDataHash32.Length != 32) { Debug.Assert(false); return; }

			byte[] pbHash = HashClipboard(); // Hash current contents
			if(pbHash == null) return; // Unknown data (i.e. no KeePass data)
			if(pbHash.Length != 32) { Debug.Assert(false); return; }

			if(!MemUtil.ArraysEqual(m_pbDataHash32, pbHash)) return;

			m_pbDataHash32 = null;
			m_strFormat = null;

			Clear();
		}

		private static byte[] HashClipboard()
		{
			try
			{
				if(Clipboard.ContainsText())
				{
					string strData = Clipboard.GetText();
					byte[] pbUtf8 = StrUtil.Utf8.GetBytes(strData);

					SHA256Managed sha256 = new SHA256Managed();
					return sha256.ComputeHash(pbUtf8);
				}
				else if(m_strFormat != null)
				{
					if(Clipboard.ContainsData(m_strFormat))
					{
						byte[] pbData;
						if(m_bEncoded) pbData = GetEncodedData(m_strFormat, IntPtr.Zero);
						else pbData = (byte[])Clipboard.GetData(m_strFormat);
						if(pbData == null) { Debug.Assert(false); return null; }

						SHA256Managed sha256 = new SHA256Managed();
						return sha256.ComputeHash(pbData);
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		public static byte[] ComputeHash()
		{
			try // This works always or never
			{
				bool bOpened = OpenW(IntPtr.Zero, false);
				// The following seems to work even without opening the
				// clipboard, but opening maybe is safer
				uint u = NativeMethods.GetClipboardSequenceNumber();
				if(bOpened) CloseW();

				if(u == 0) throw new UnauthorizedAccessException();

				SHA256Managed sha256 = new SHA256Managed();
				return sha256.ComputeHash(MemUtil.UInt32ToBytes(u));
			}
			catch(Exception) { Debug.Assert(false); }

			try
			{
				MemoryStream ms = new MemoryStream();

				byte[] pbPre = StrUtil.Utf8.GetBytes("pb");
				ms.Write(pbPre, 0, pbPre.Length); // Prevent empty buffer

				if(Clipboard.ContainsAudio())
				{
					Stream sAudio = Clipboard.GetAudioStream();
					MemUtil.CopyStream(sAudio, ms);
					sAudio.Close();
				}
				if(Clipboard.ContainsFileDropList())
				{
					StringCollection sc = Clipboard.GetFileDropList();
					foreach(string str in sc)
					{
						byte[] pbStr = StrUtil.Utf8.GetBytes(str);
						ms.Write(pbStr, 0, pbStr.Length);
					}
				}
				if(Clipboard.ContainsImage())
				{
					using(Image img = Clipboard.GetImage())
					{
						MemoryStream msImage = new MemoryStream();
						img.Save(msImage, ImageFormat.Bmp);
						byte[] pbImg = msImage.ToArray();
						ms.Write(pbImg, 0, pbImg.Length);
						msImage.Close();
					}
				}
				if(Clipboard.ContainsText())
				{
					string str = Clipboard.GetText();
					byte[] pbText = StrUtil.Utf8.GetBytes(str);
					ms.Write(pbText, 0, pbText.Length);
				}

				byte[] pbData = ms.ToArray();
				SHA256Managed sha256 = new SHA256Managed();
				byte[] pbHash = sha256.ComputeHash(pbData);
				ms.Close();

				return pbHash;
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		private static DataObject CreateProtectedDataObject(string strText)
		{
			DataObject d = new DataObject();
			AttachIgnoreFormat(d);

			Debug.Assert(strText != null); if(strText == null) return d;

			if(strText.Length > 0) d.SetText(strText);
			return d;
		}

		private static DataObject CreateProtectedDataObject(string strFormat,
			byte[] pbData)
		{
			DataObject d = new DataObject();
			AttachIgnoreFormat(d);

			Debug.Assert(strFormat != null); if(strFormat == null) return d;
			Debug.Assert(pbData != null); if(pbData == null) return d;

			if(pbData.Length > 0) d.SetData(strFormat, pbData);
			return d;
		}

		private static void AttachIgnoreFormat(DataObject doData)
		{
			Debug.Assert(doData != null); if(doData == null) return;

			if(!Program.Config.Security.UseClipboardViewerIgnoreFormat) return;
			if(KeeNativeLib.NativeLib.IsUnix()) return; // Not supported on Unix

			try
			{
				doData.SetData(ClipboardIgnoreFormatName, false, PwDefs.ProductName);
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
