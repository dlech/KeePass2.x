/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Cryptography.KeyDerivation;
using KeePassLib.Resources;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class DatabaseSettingsForm : Form
	{
		private bool m_bCreatingNew = false;
		private PwDatabase m_pwDatabase = null;

		private readonly string m_strAutoCreateNew = "(" + KPRes.AutoCreateNew + ")";
		private readonly Dictionary<int, PwUuid> m_dictRecycleBinGroups =
			new Dictionary<int, PwUuid>();
		private readonly Dictionary<int, PwUuid> m_dictEntryTemplateGroups =
			new Dictionary<int, PwUuid>();

		private bool m_bInitializing = true;
		private volatile Thread m_thKdf = null;

		public PwDatabase DatabaseEx
		{
			get { return m_pwDatabase; }
		}

		public DatabaseSettingsForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		public void InitEx(bool bCreatingNew, PwDatabase pwDatabase)
		{
			m_bCreatingNew = bCreatingNew;

			Debug.Assert(pwDatabase != null); if(pwDatabase == null) throw new ArgumentNullException("pwDatabase");
			m_pwDatabase = pwDatabase;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			if(m_pwDatabase == null) { Debug.Assert(false); throw new InvalidOperationException(); }

			m_bInitializing = true;

			GlobalWindowManager.AddWindow(this);

			IOConnectionInfo ioc = m_pwDatabase.IOConnectionInfo;
			string strDisp = ioc.GetDisplayName();

			string strDesc = KPRes.DatabaseSettingsDesc;
			if(!string.IsNullOrEmpty(strDisp)) strDesc = strDisp;

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_Ark, KPRes.DatabaseSettings, strDesc);
			this.Icon = AppIcons.Default;

			FontUtil.AssignDefaultBold(m_rbCmpNone);
			FontUtil.AssignDefaultBold(m_rbCmpGZip);

			UIUtil.ConfigureToolTip(m_ttRect);
			UIUtil.SetToolTip(m_ttRect, m_btnKdf1Sec, KPRes.KdfParams1Sec, false);

			AccessibilityEx.SetContext(m_numHistoryMaxItems, m_cbHistoryMaxItems);
			AccessibilityEx.SetContext(m_numHistoryMaxSize, m_cbHistoryMaxSize);
			AccessibilityEx.SetContext(m_numKeyRecDays, m_cbKeyRec);
			AccessibilityEx.SetContext(m_numKeyForceDays, m_cbKeyForce);

			m_tbDbName.PromptText = KPRes.DatabaseNamePrompt;
			m_tbDbDesc.PromptText = KPRes.DatabaseDescPrompt;

			if(m_bCreatingNew) this.Text = KPRes.ConfigureOnNewDatabase3;
			else this.Text = KPRes.DatabaseSettings;

			m_tbDbName.Text = m_pwDatabase.Name;
			UIUtil.SetMultilineText(m_tbDbDesc, m_pwDatabase.Description);
			m_tbDefaultUser.Text = m_pwDatabase.DefaultUserName;

			m_btnColor.Colors = AppIcons.Colors;

			Color clr = m_pwDatabase.Color;
			bool bClr = !UIUtil.ColorsEqual(clr, Color.Empty);
			if(bClr) m_btnColor.SelectedColor = AppIcons.RoundColor(clr);
			m_cbColor.Checked = bClr;

			for(int inx = 0; inx < CipherPool.GlobalPool.EngineCount; ++inx)
				m_cmbEncAlgo.Items.Add(CipherPool.GlobalPool[inx].DisplayName);

			if(m_cmbEncAlgo.Items.Count > 0)
			{
				int nIndex = CipherPool.GlobalPool.GetCipherIndex(m_pwDatabase.DataCipherUuid);
				m_cmbEncAlgo.SelectedIndex = ((nIndex >= 0) ? nIndex : 0);
			}

			Debug.Assert(m_cmbKdf.Items.Count == 0);
			foreach(KdfEngine kdf in KdfPool.Engines)
			{
				m_cmbKdf.Items.Add(kdf.Name);
			}

			m_numKdfIt.Minimum = ulong.MinValue;
			m_numKdfIt.Maximum = ulong.MaxValue;

			m_numKdfMem.Minimum = ulong.MinValue;
			m_numKdfMem.Maximum = ulong.MaxValue;

			Debug.Assert(m_cmbKdfMem.Items.Count == 0);
			Debug.Assert(!m_cmbKdfMem.Sorted);
			m_cmbKdfMem.Items.Add("B");
			m_cmbKdfMem.Items.Add("KB");
			m_cmbKdfMem.Items.Add("MB");
			m_cmbKdfMem.Items.Add("GB");

			m_numKdfPar.Minimum = uint.MinValue;
			m_numKdfPar.Maximum = uint.MaxValue;

			SetKdfParameters(m_pwDatabase.KdfParameters);

			// m_lbMemProt.Items.Add(KPRes.Title, m_pwDatabase.MemoryProtection.ProtectTitle);
			// m_lbMemProt.Items.Add(KPRes.UserName, m_pwDatabase.MemoryProtection.ProtectUserName);
			// m_lbMemProt.Items.Add(KPRes.Password, m_pwDatabase.MemoryProtection.ProtectPassword);
			// m_lbMemProt.Items.Add(KPRes.Url, m_pwDatabase.MemoryProtection.ProtectUrl);
			// m_lbMemProt.Items.Add(KPRes.Notes, m_pwDatabase.MemoryProtection.ProtectNotes);

			// m_cbAutoEnableHiding.Checked = m_pwDatabase.MemoryProtection.AutoEnableVisualHiding;
			// m_cbAutoEnableHiding.Checked = false;

			if(m_pwDatabase.Compression == PwCompressionAlgorithm.None)
				m_rbCmpNone.Checked = true;
			else if(m_pwDatabase.Compression == PwCompressionAlgorithm.GZip)
				m_rbCmpGZip.Checked = true;
			else { Debug.Assert(false); }

			InitRecycleBinTab();
			InitAdvancedTab();

			m_bInitializing = false;
			EnableControlsEx();
			UIUtil.SetFocus(m_tbDbName, this);
		}

		private void InitRecycleBinTab()
		{
			m_cbRecycleBin.Checked = m_pwDatabase.RecycleBinEnabled;

			m_cmbRecycleBin.Items.Add(m_strAutoCreateNew);
			m_dictRecycleBinGroups[0] = PwUuid.Zero;

			int iSelect;
			UIUtil.CreateGroupList(m_pwDatabase.RootGroup, m_cmbRecycleBin,
				m_dictRecycleBinGroups, m_pwDatabase.RecycleBinUuid, out iSelect);

			m_cmbRecycleBin.SelectedIndex = Math.Max(0, iSelect);
		}

		private void InitAdvancedTab()
		{
			m_cmbEntryTemplates.Items.Add("(" + KPRes.None + ")");
			m_dictEntryTemplateGroups[0] = PwUuid.Zero;

			int iSelect;
			UIUtil.CreateGroupList(m_pwDatabase.RootGroup, m_cmbEntryTemplates,
				m_dictEntryTemplateGroups, m_pwDatabase.EntryTemplatesGroup, out iSelect);

			m_cmbEntryTemplates.SelectedIndex = Math.Max(0, iSelect);

			m_numHistoryMaxItems.Minimum = 0;
			m_numHistoryMaxItems.Maximum = int.MaxValue;
			bool bHistMaxItems = (m_pwDatabase.HistoryMaxItems >= 0);
			m_numHistoryMaxItems.Value = (bHistMaxItems ? m_pwDatabase.HistoryMaxItems :
				PwDatabase.DefaultHistoryMaxItems);
			m_cbHistoryMaxItems.Checked = bHistMaxItems;

			m_numHistoryMaxSize.Minimum = 0;
			m_numHistoryMaxSize.Maximum = long.MaxValue / (1024 * 1024);
			bool bHistMaxSize = (m_pwDatabase.HistoryMaxSize >= 0);
			m_numHistoryMaxSize.Value = (bHistMaxSize ? m_pwDatabase.HistoryMaxSize :
				PwDatabase.DefaultHistoryMaxSize) / (1024 * 1024);
			m_cbHistoryMaxSize.Checked = bHistMaxSize;

			m_numKeyRecDays.Minimum = 0;
			m_numKeyRecDays.Maximum = long.MaxValue;
			bool bChangeRec = (m_pwDatabase.MasterKeyChangeRec >= 0);
			m_numKeyRecDays.Value = (bChangeRec ? m_pwDatabase.MasterKeyChangeRec : 182);
			m_cbKeyRec.Checked = bChangeRec;

			m_numKeyForceDays.Minimum = 0;
			m_numKeyForceDays.Maximum = long.MaxValue;
			bool bChangeForce = (m_pwDatabase.MasterKeyChangeForce >= 0);
			m_numKeyForceDays.Value = (bChangeForce ? m_pwDatabase.MasterKeyChangeForce : 365);
			m_cbKeyForce.Checked = bChangeForce;

			m_cbKeyForceOnce.Checked = m_pwDatabase.MasterKeyChangeForceOnce;
		}

		private void EnableControlsEx()
		{
			if(m_bInitializing) return;

			m_btnColor.Enabled = m_cbColor.Checked;

			KdfEngine kdf = GetKdf();
			if(kdf != null)
			{
				if(kdf is AesKdf)
				{
					m_lblKdfMem.Visible = false;
					m_numKdfMem.Visible = false;
					m_cmbKdfMem.Visible = false;

					m_lblKdfPar.Visible = false;
					m_numKdfPar.Visible = false;
				}
				else if(kdf is Argon2Kdf)
				{
					m_lblKdfMem.Visible = true;
					m_numKdfMem.Visible = true;
					m_cmbKdfMem.Visible = true;

					m_lblKdfPar.Visible = true;
					m_numKdfPar.Visible = true;
				}
				// else { Debug.Assert(false); } // Plugins may provide controls
			}
			else { Debug.Assert(false); }

			m_numHistoryMaxItems.Enabled = m_cbHistoryMaxItems.Checked;
			m_numHistoryMaxSize.Enabled = m_cbHistoryMaxSize.Checked;

			bool bEnableDays = ((Program.Config.UI.UIFlags &
				(ulong)AceUIFlags.DisableKeyChangeDays) == 0);
			m_numKeyRecDays.Enabled = (bEnableDays && m_cbKeyRec.Checked);
			m_numKeyForceDays.Enabled = (bEnableDays && m_cbKeyForce.Checked);
			m_cbKeyRec.Enabled = bEnableDays;
			m_cbKeyForce.Enabled = bEnableDays;
			m_cbKeyForceOnce.Enabled = bEnableDays;

			bool bKdfTh = (m_thKdf != null);
			m_tabMain.Enabled = !bKdfTh;

			m_pbKdf.Visible = bKdfTh;

			m_btnHelp.Enabled = !bKdfTh;
			m_btnCancelOp.Enabled = bKdfTh;
			m_btnCancelOp.Visible = bKdfTh;
			m_btnOK.Enabled = !bKdfTh;
			m_btnCancel.Enabled = !bKdfTh;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			m_pwDatabase.SettingsChanged = DateTime.UtcNow;

			if(m_tbDbName.Text != m_pwDatabase.Name)
			{
				m_pwDatabase.Name = m_tbDbName.Text;
				m_pwDatabase.NameChanged = DateTime.UtcNow;
			}

			string strNew = m_tbDbDesc.Text;
			string strOrgFlt = StrUtil.NormalizeNewLines(m_pwDatabase.Description, false);
			string strNewFlt = StrUtil.NormalizeNewLines(strNew, false);
			if(strNewFlt != strOrgFlt)
			{
				m_pwDatabase.Description = strNew;
				m_pwDatabase.DescriptionChanged = DateTime.UtcNow;
			}

			if(m_tbDefaultUser.Text != m_pwDatabase.DefaultUserName)
			{
				m_pwDatabase.DefaultUserName = m_tbDefaultUser.Text;
				m_pwDatabase.DefaultUserNameChanged = DateTime.UtcNow;
			}

			m_pwDatabase.Color = (m_cbColor.Checked ? m_btnColor.SelectedColor :
				Color.Empty);

			int nCipher = CipherPool.GlobalPool.GetCipherIndex(m_cmbEncAlgo.Text);
			Debug.Assert(nCipher >= 0);
			if(nCipher >= 0)
				m_pwDatabase.DataCipherUuid = CipherPool.GlobalPool[nCipher].CipherUuid;
			else
				m_pwDatabase.DataCipherUuid = StandardAesEngine.AesUuid;

			// m_pwDatabase.KeyEncryptionRounds = (ulong)m_numKdfIt.Value;
			KdfParameters pKdf = GetKdfParameters(true, true);
			if(pKdf != null) m_pwDatabase.KdfParameters = pKdf;
			// No assert, plugins may assign KDF parameters

			if(m_rbCmpNone.Checked) m_pwDatabase.Compression = PwCompressionAlgorithm.None;
			else if(m_rbCmpGZip.Checked) m_pwDatabase.Compression = PwCompressionAlgorithm.GZip;
			else { Debug.Assert(false); }

			// m_pwDatabase.MemoryProtection.ProtectTitle = UpdateMemoryProtection(0,
			//	m_pwDatabase.MemoryProtection.ProtectTitle, PwDefs.TitleField);
			// m_pwDatabase.MemoryProtection.ProtectUserName = UpdateMemoryProtection(1,
			//	m_pwDatabase.MemoryProtection.ProtectUserName, PwDefs.UserNameField);
			// m_pwDatabase.MemoryProtection.ProtectPassword = UpdateMemoryProtection(2,
			//	m_pwDatabase.MemoryProtection.ProtectPassword, PwDefs.PasswordField);
			// m_pwDatabase.MemoryProtection.ProtectUrl = UpdateMemoryProtection(3,
			//	m_pwDatabase.MemoryProtection.ProtectUrl, PwDefs.UrlField);
			// m_pwDatabase.MemoryProtection.ProtectNotes = UpdateMemoryProtection(4,
			//	m_pwDatabase.MemoryProtection.ProtectNotes, PwDefs.NotesField);

			// m_pwDatabase.MemoryProtection.AutoEnableVisualHiding = m_cbAutoEnableHiding.Checked;

			if(m_cbRecycleBin.Checked != m_pwDatabase.RecycleBinEnabled)
			{
				m_pwDatabase.RecycleBinEnabled = m_cbRecycleBin.Checked;
				m_pwDatabase.RecycleBinChanged = DateTime.UtcNow;
			}
			int iRecBinSel = m_cmbRecycleBin.SelectedIndex;
			if(m_dictRecycleBinGroups.ContainsKey(iRecBinSel))
			{
				if(!m_dictRecycleBinGroups[iRecBinSel].Equals(m_pwDatabase.RecycleBinUuid))
				{
					m_pwDatabase.RecycleBinUuid = m_dictRecycleBinGroups[iRecBinSel];
					m_pwDatabase.RecycleBinChanged = DateTime.UtcNow;
				}
			}
			else
			{
				Debug.Assert(false);
				if(!PwUuid.Zero.Equals(m_pwDatabase.RecycleBinUuid))
				{
					m_pwDatabase.RecycleBinUuid = PwUuid.Zero;
					m_pwDatabase.RecycleBinChanged = DateTime.UtcNow;
				}
			}

			int iTemplSel = m_cmbEntryTemplates.SelectedIndex;
			if(m_dictEntryTemplateGroups.ContainsKey(iTemplSel))
			{
				if(!m_dictEntryTemplateGroups[iTemplSel].Equals(m_pwDatabase.EntryTemplatesGroup))
				{
					m_pwDatabase.EntryTemplatesGroup = m_dictEntryTemplateGroups[iTemplSel];
					m_pwDatabase.EntryTemplatesGroupChanged = DateTime.UtcNow;
				}
			}
			else
			{
				Debug.Assert(false);
				if(!PwUuid.Zero.Equals(m_pwDatabase.EntryTemplatesGroup))
				{
					m_pwDatabase.EntryTemplatesGroup = PwUuid.Zero;
					m_pwDatabase.EntryTemplatesGroupChanged = DateTime.UtcNow;
				}
			}

			if(!m_cbHistoryMaxItems.Checked) m_pwDatabase.HistoryMaxItems = -1;
			else m_pwDatabase.HistoryMaxItems = (int)m_numHistoryMaxItems.Value;

			if(!m_cbHistoryMaxSize.Checked) m_pwDatabase.HistoryMaxSize = -1;
			else m_pwDatabase.HistoryMaxSize = (long)m_numHistoryMaxSize.Value * 1024 * 1024;

			m_pwDatabase.MaintainBackups(); // Apply new history settings

			if(!m_cbKeyRec.Checked) m_pwDatabase.MasterKeyChangeRec = -1;
			else m_pwDatabase.MasterKeyChangeRec = (long)m_numKeyRecDays.Value;

			if(!m_cbKeyForce.Checked) m_pwDatabase.MasterKeyChangeForce = -1;
			else m_pwDatabase.MasterKeyChangeForce = (long)m_numKeyForceDays.Value;

			m_pwDatabase.MasterKeyChangeForceOnce = m_cbKeyForceOnce.Checked;
		}

		// private bool UpdateMemoryProtection(int nIndex, bool bOldSetting,
		//	string strFieldID)
		// {
		//	bool bNewProt = m_lbMemProt.GetItemChecked(nIndex);
		//	if(bNewProt != bOldSetting)
		//		m_pwDatabase.RootGroup.EnableStringFieldProtection(strFieldID, bNewProt);
		// #if DEBUG
		//	EntryHandler eh = delegate(PwEntry pe)
		//	{
		//		ProtectedString ps = pe.Strings.Get(strFieldID);
		//		if(ps != null) { Debug.Assert(ps.IsProtected == bNewProt); }
		//		return true;
		//	};
		//	Debug.Assert(m_pwDatabase.RootGroup.TraverseTree(
		//		TraversalMethod.PreOrder, null, eh));
		// #endif
		//	return bNewProt;
		// }

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			string strSubTopic = null;
			if(m_tabMain.SelectedTab == m_tabGeneral)
				strSubTopic = AppDefs.HelpTopics.DbSettingsGeneral;
			else if(m_tabMain.SelectedTab == m_tabSecurity)
				strSubTopic = AppDefs.HelpTopics.DbSettingsSecurity;
			// else if(m_tabMain.SelectedTab == m_tabProtection)
			//	strSubTopic = AppDefs.HelpTopics.DbSettingsProtection;
			else if(m_tabMain.SelectedTab == m_tabCompression)
				strSubTopic = AppDefs.HelpTopics.DbSettingsCompression;

			AppHelp.ShowHelp(AppDefs.HelpTopics.DatabaseSettings, strSubTopic);
		}

		private bool AbortKdfThread()
		{
			if(m_thKdf == null) return false;

			try { m_thKdf.Abort(); }
			catch(Exception) { Debug.Assert(false); }
			m_thKdf = null;

			return true;
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			if(AbortKdfThread()) { Debug.Assert(false); }

			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnKeyRecCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnKeyForceCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		// private void OnLinkClickedMemProtHelp(object sender, LinkLabelLinkClickedEventArgs e)
		// {
		//	AppHelp.ShowHelp(AppDefs.HelpTopics.FaqTech, AppDefs.HelpTopics.FaqTechMemProt);
		// }

		private void OnHistoryMaxItemsCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnHistoryMaxSizeCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnColorCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private KdfEngine GetKdf()
		{
			return KdfPool.Get(m_cmbKdf.Text);
		}

		private KdfParameters GetKdfParameters(bool bShowAdjustments, bool bAdjustWeak)
		{
			KdfEngine kdf = GetKdf();
			if(kdf == null) { Debug.Assert(false); return null; }

			string strAdj = string.Empty;

			KdfParameters p = kdf.GetDefaultParameters();
			if(kdf is AesKdf)
				p.SetUInt64(AesKdf.ParamRounds, (ulong)m_numKdfIt.Value);
			else if(kdf is Argon2Kdf)
			{
				ulong uIt = (ulong)m_numKdfIt.Value;
				AdjustKdfParam<ulong>(ref uIt, ">=", Argon2Kdf.MinIterations,
					KPRes.Iterations, ref strAdj);
				AdjustKdfParam<ulong>(ref uIt, "<=", Argon2Kdf.MaxIterations,
					KPRes.Iterations, ref strAdj);
				p.SetUInt64(Argon2Kdf.ParamIterations, uIt);

				// Adjust parallelism first, as memory depends on it
				uint uPar = (uint)m_numKdfPar.Value;
				AdjustKdfParam<uint>(ref uPar, ">=", Argon2Kdf.MinParallelism,
					KPRes.Parallelism, ref strAdj);
				uint uParMax = Argon2Kdf.MaxParallelism;
				int cp = Environment.ProcessorCount;
				if((cp > 0) && (cp <= (int.MaxValue / 2)))
					uParMax = Math.Min(uParMax, (uint)(cp * 2));
				AdjustKdfParam<uint>(ref uPar, "<=", uParMax,
					KPRes.Parallelism, ref strAdj);
				p.SetUInt32(Argon2Kdf.ParamParallelism, uPar);

				ulong uMem = (ulong)m_numKdfMem.Value;
				int iMemUnit = m_cmbKdfMem.SelectedIndex;
				while(iMemUnit > 0)
				{
					if(uMem > (ulong.MaxValue / 1024UL))
					{
						uMem = ulong.MaxValue;
						break;
					}

					uMem *= 1024UL;
					--iMemUnit;
				}

				// 8*p blocks = 1024*8*p bytes minimum memory, see spec.
				Debug.Assert(Argon2Kdf.MinMemory == (1024UL * 8UL));
				ulong uMemMin = Argon2Kdf.MinMemory * uPar;
				AdjustKdfParam<ulong>(ref uMem, ">=", uMemMin,
					KPRes.Memory, ref strAdj);
				AdjustKdfParam<ulong>(ref uMem, "<=", Argon2Kdf.MaxMemory,
					KPRes.Memory, ref strAdj);
				p.SetUInt64(Argon2Kdf.ParamMemory, uMem);
			}
			else return null; // Plugins may handle it

			if(bShowAdjustments && (strAdj.Length != 0))
			{
				strAdj = KPRes.KdfAdjust + MessageService.NewParagraph + strAdj;
				MessageService.ShowInfo(strAdj);
			}

			if(bAdjustWeak) KeyUtil.KdfAdjustWeakParameters(ref p, null);

			return p;
		}

		private static void AdjustKdfParam<T>(ref T tValue, string strReq,
			T tCmp, string strName, ref string strAdj)
			where T : struct, IComparable<T>
		{
			if(strReq == ">=")
			{
				if(tValue.CompareTo(tCmp) >= 0) return;
			}
			else if(strReq == "<=")
			{
				if(tValue.CompareTo(tCmp) <= 0) return;
			}
			else { Debug.Assert(false); return; }

			if(strAdj.Length > 0)
				strAdj += MessageService.NewLine;
			strAdj += "* " + strName + ": " + tValue.ToString() +
				" -> " + tCmp.ToString() + ".";

			tValue = tCmp;
		}

		private void SetKdfParameters(KdfParameters p)
		{
			if(p == null) { Debug.Assert(false); return; }

			KdfEngine kdf = KdfPool.Get(p.KdfUuid);
			if(kdf == null) { Debug.Assert(false); return; }

			for(int i = 0; i < m_cmbKdf.Items.Count; ++i)
			{
				string strKdf = (m_cmbKdf.Items[i] as string);
				if(string.IsNullOrEmpty(strKdf)) { Debug.Assert(false); continue; }

				if(strKdf.Equals(kdf.Name, StrUtil.CaseIgnoreCmp))
				{
					bool bPrevInit = m_bInitializing;
					m_bInitializing = true; // Prevent selection handler

					m_cmbKdf.SelectedIndex = i;

					m_bInitializing = bPrevInit;
					break;
				}
			}

			if(kdf is AesKdf)
			{
				ulong uIt = p.GetUInt64(AesKdf.ParamRounds,
					PwDefs.DefaultKeyEncryptionRounds);
				SetKdfParameters(uIt, 1024, 2);
			}
			else if(kdf is Argon2Kdf)
			{
				ulong uIt = p.GetUInt64(Argon2Kdf.ParamIterations,
					Argon2Kdf.DefaultIterations);
				ulong uMem = p.GetUInt64(Argon2Kdf.ParamMemory,
					Argon2Kdf.DefaultMemory);
				uint uPar = p.GetUInt32(Argon2Kdf.ParamParallelism,
					Argon2Kdf.DefaultParallelism);
				SetKdfParameters(uIt, uMem, uPar);
			}
			// else { Debug.Assert(false); } // Plugins may provide other KDFs
		}

		private void SetKdfParameters(ulong uIt, ulong uMem, uint uPar)
		{
			m_numKdfIt.Value = uIt;

			int nUnits = m_cmbKdfMem.Items.Count;
			if((nUnits == 0) || (uMem == 0))
			{
				m_numKdfMem.Value = uMem;
				if(nUnits > 0) m_cmbKdfMem.SelectedIndex = 0;
				else { Debug.Assert(false); }
				return;
			}

			ulong uMemWrtUnit = uMem;
			int iUnit = 0;
			while(iUnit < (nUnits - 1))
			{
				if((uMemWrtUnit % 1024UL) != 0UL) break;

				uMemWrtUnit /= 1024UL;
				++iUnit;
			}

			m_numKdfMem.Value = uMemWrtUnit;
			m_cmbKdfMem.SelectedIndex = iUnit;

			m_numKdfPar.Value = uPar;
		}

		private void OnKdfSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_bInitializing) return;

			KdfEngine kdf = GetKdf();
			if(kdf == null) { Debug.Assert(false); return; }

			SetKdfParameters(kdf.GetDefaultParameters());
			EnableControlsEx();
		}

		private void OnBtnKdf1Sec(object sender, EventArgs e)
		{
			KdfEngine kdf = GetKdf();
			if(kdf == null) { Debug.Assert(false); return; }

			if(!(kdf is AesKdf) && !(kdf is Argon2Kdf)) return; // No assert, plugins
			if(m_thKdf != null) { Debug.Assert(false); return; }

			try
			{
				m_thKdf = new Thread(new ParameterizedThreadStart(this.Kdf1SecTh));
				EnableControlsEx(); // Disable controls (m_thKdf is not null)

				m_thKdf.Start(kdf);
			}
			catch(Exception)
			{
				Debug.Assert(false);
				m_thKdf = null;
				EnableControlsEx();
			}
		}

		private void Kdf1SecTh(object o)
		{
			KdfParameters p = null;
			string strMsg = null;

			try
			{
				KdfEngine kdf = (o as KdfEngine);
				if(kdf != null) p = kdf.GetBestParameters(1000);
				else { Debug.Assert(false); }
			}
			catch(ThreadAbortException)
			{
				try { Thread.ResetAbort(); }
				catch(Exception) { Debug.Assert(false); }
				return;
			}
			catch(Exception ex)
			{
				if(!string.IsNullOrEmpty(ex.Message)) strMsg = ex.Message;
			}
			finally { m_thKdf = null; } // Before continuation, to enable controls

			try { m_btnOK.Invoke(new KdfpDelegate(this.Kdf1SecPost), p, strMsg); }
			catch(Exception) { Debug.Assert(false); }
		}

		private delegate void KdfpDelegate(KdfParameters p, string strMsg);

		private void Kdf1SecPost(KdfParameters p, string strMsg)
		{
			try
			{
				Debug.Assert(!m_btnOK.InvokeRequired);

				if(!string.IsNullOrEmpty(strMsg))
					MessageService.ShowInfo(strMsg);
				else SetKdfParameters(p);

				EnableControlsEx();
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private void OnBtnKdfTest(object sender, EventArgs e)
		{
			KdfParameters p = GetKdfParameters(true, false);
			if(p == null) return; // No assert, plugins

			if(m_thKdf != null) { Debug.Assert(false); return; }

			try
			{
				SetKdfParameters(p); // Show auto-adjusted parameters

				m_thKdf = new Thread(new ParameterizedThreadStart(this.KdfTestTh));
				EnableControlsEx(); // Disable controls (m_thKdf is not null)

				m_thKdf.Start(p);
			}
			catch(Exception)
			{
				Debug.Assert(false);
				m_thKdf = null;
				EnableControlsEx();
			}
		}

		private void KdfTestTh(object o)
		{
			string strMsg = KLRes.UnknownError;

			try
			{
				KdfParameters p = (o as KdfParameters);
				if(p == null) { Debug.Assert(false); return; }

				KdfEngine kdf = KdfPool.Get(p.KdfUuid);
				if(kdf == null) { Debug.Assert(false); return; }

				ulong uTimeMS;
				if(!KeyUtil.KdfPrcTest(p, out uTimeMS))
					uTimeMS = kdf.Test(p);

				if(uTimeMS == 0) uTimeMS = 1;
				double dTimeS = (double)uTimeMS / 1000.0;

				strMsg = KPRes.TestSuccess + MessageService.NewParagraph +
					KPRes.TransformTime.Replace(@"{PARAM}", dTimeS.ToString());
			}
			catch(ThreadAbortException)
			{
				try { Thread.ResetAbort(); }
				catch(Exception) { Debug.Assert(false); }
				return;
			}
			catch(Exception ex)
			{
				Debug.Assert(false);
				if(!string.IsNullOrEmpty(ex.Message)) strMsg = ex.Message;
			}
			finally { m_thKdf = null; } // Before continuation, to enable controls

			try { m_btnOK.Invoke(new KdfsDelegate(this.KdfTestPost), strMsg); }
			catch(Exception) { Debug.Assert(false); }
		}

		private delegate void KdfsDelegate(string strMsg);

		private void KdfTestPost(string strMsg)
		{
			try
			{
				Debug.Assert(!m_btnOK.InvokeRequired);

				EnableControlsEx();

				if(!string.IsNullOrEmpty(strMsg))
					MessageService.ShowInfo(strMsg);
				else { Debug.Assert(false); }
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private void OnBtnCancelOp(object sender, EventArgs e)
		{
			AbortKdfThread();
			EnableControlsEx();
		}
	}
}
