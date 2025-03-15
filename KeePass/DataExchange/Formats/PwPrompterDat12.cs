/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.Forms;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Resources;

namespace KeePass.DataExchange.Formats
{
	internal sealed class PwPrompterDat12 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Password Prompter DAT"; } }
		public override string DefaultExtension { get { return "dat"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			SingleLineEditForm dlg = new SingleLineEditForm();
			dlg.InitEx(KPRes.Password, KPRes.Import + ": " + this.FormatName,
				KPRes.PasswordPrompt, Properties.Resources.B48x48_KGPG_Key2,
				string.Empty, null);
			dlg.FlagsEx |= SlfFlags.Sensitive;
			if(UIUtil.ShowDialogNotValue(dlg, DialogResult.OK)) return;
			string strPassword = dlg.ResultString;
			UIUtil.DestroyForm(dlg);

			byte[] pbPassword = Encoding.Default.GetBytes(strPassword);

			using(BinaryReader br = new BinaryReader(sInput, Encoding.Default))
			{
				Import(pdStorage, br, pbPassword);
			}
		}

		private void Import(PwDatabase pd, BinaryReader br, byte[] pbPassword)
		{
			ushort usFileVersion = br.ReadUInt16();
			if(usFileVersion != 0x0100)
				throw new Exception(KLRes.FileVersionUnsupported);

			uint uEntries = br.ReadUInt32();
			uint uKeySize = br.ReadUInt32();
			Debug.Assert(uKeySize == 50); // It's a constant
			
			byte btKeyArrayLen = br.ReadByte();
			byte[] pbKey = br.ReadBytes(btKeyArrayLen);

			byte btValidArrayLen = br.ReadByte();
			byte[] pbValid = br.ReadBytes(btValidArrayLen);

			if(pbPassword.Length != 0)
			{
				MangleSetKey(pbPassword);
				MangleDecode(pbKey);
			}

			MangleSetKey(pbKey);
			MangleDecode(pbValid);
			string strValid = Encoding.Default.GetString(pbValid);
			if(strValid != "aacaaaadaaeabaacyuioqaqqaaaaaertaaajkadaadaaxywqea")
				throw new Exception(KLRes.InvalidCompositeKey);

			for(uint uEntry = 0; uEntry < uEntries; ++uEntry)
			{
				PwEntry pe = new PwEntry(true, true);
				pd.RootGroup.AddEntry(pe, true);

				ImportUtil.Add(pe, PwDefs.TitleField, ReadString(br), pd);
				ImportUtil.Add(pe, PwDefs.UserNameField, ReadString(br), pd);
				ImportUtil.Add(pe, PwDefs.PasswordField, ReadString(br), pd);
				ImportUtil.Add(pe, "Hint", ReadString(br), pd);
				ImportUtil.Add(pe, PwDefs.NotesField, ReadString(br), pd);
				ImportUtil.Add(pe, PwDefs.UrlField, ReadString(br), pd);
			}
		}

		private string ReadString(BinaryReader br)
		{
			byte btLen = br.ReadByte();
			byte[] pbData = br.ReadBytes(btLen);

			MangleDecode(pbData);

			return Encoding.Default.GetString(pbData);
		}

		byte[] m_pbMangleKey = null;
		private void MangleSetKey(byte[] pbKey)
		{
			if(pbKey == null) { Debug.Assert(false); return; }

			m_pbMangleKey = new byte[pbKey.Length];
			Array.Copy(pbKey, m_pbMangleKey, pbKey.Length);
		}

		private void MangleDecode(byte[] pbData)
		{
			if(m_pbMangleKey == null) { Debug.Assert(false); return; }

			int nKeyIndex = 0, nIndex = 0, nRemLen = pbData.Length;
			bool bUp = true;

			while(nRemLen > 0)
			{
				if(nKeyIndex > (m_pbMangleKey.Length - 1))
				{
					nKeyIndex = m_pbMangleKey.Length - 1;
					bUp = false;
				}
				else if(nKeyIndex < 0)
				{
					nKeyIndex = 0;
					bUp = true;
				}

				pbData[nIndex] ^= m_pbMangleKey[nKeyIndex];

				nKeyIndex += (bUp ? 1 : -1);

				++nIndex;
				--nRemLen;
			}
		}
	}
}
