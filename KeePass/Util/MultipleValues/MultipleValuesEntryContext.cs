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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

using KeePass.UI;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Util.MultipleValues
{
	public sealed class MultipleValuesEntryContext : MultipleValuesContext
	{
		private readonly PwEntry[] m_v;
		private readonly PwDatabase m_pd; // Impl. detail; maybe entries from diff. dbs. in the future
		private readonly PwEntry m_peM;

		private readonly PwUuid m_puCueIcon = new PwUuid(true);

		private readonly bool[] m_vModified;

		internal readonly Dictionary<string, bool> MultiStringProt =
			new Dictionary<string, bool>();
		internal bool MultiExpiry = false;
		internal bool MultiFgColor = false;
		internal bool MultiBgColor = false;
		internal bool MultiAutoTypeEnabled = false;
		internal bool MultiAutoTypeObf = false;

		// For plugins
		public PwEntry[] Entries
		{
			get { return m_v; }
		}

		internal MultipleValuesEntryContext(PwEntry[] v, PwDatabase pd, out PwEntry peMulti)
		{
			if((v == null) || (v.Length < 1)) throw new ArgumentOutOfRangeException("v");
			if(Array.IndexOf(v, null) >= 0) throw new ArgumentOutOfRangeException("v");
			if(pd == null) throw new ArgumentNullException("pd");
			if(!pd.IsOpen) throw new ArgumentOutOfRangeException("pd");

			PwEntry peM = new PwEntry(true, true);
			peM.ParentGroup = pd.RootGroup;

			m_v = new PwEntry[v.Length];
			Array.Copy(v, m_v, v.Length);
			m_pd = pd;
			m_peM = peM;

			m_vModified = new bool[v.Length];

			MultiInitStrings();
			MultiInitIcon();
			MultiInitProperties();
			MultiInitCustomData();

			if(peM.CustomIconUuid.Equals(m_puCueIcon))
			{
				using(Image img = MultipleValuesEx.CreateMultiImage(null))
				{
					using(MemoryStream ms = new MemoryStream())
					{
						img.Save(ms, ImageFormat.Png);

						PwCustomIcon ico = new PwCustomIcon(m_puCueIcon, ms.ToArray());
						ico.Name = MultipleValuesEx.CueString;

						pd.CustomIcons.Add(ico);
						pd.UINeedsIconUpdate = true;
					}
				}
			}

			peMulti = peM;
		}

		protected override void Dispose(bool disposing)
		{
			if(!disposing) { Debug.Assert(false); return; }
			if(!m_pd.IsOpen) { Debug.Assert(false); return; }

			int i = m_pd.GetCustomIconIndex(m_puCueIcon);
			if(i >= 0)
			{
				m_pd.CustomIcons.RemoveAt(i);
				m_pd.UINeedsIconUpdate = true;
			}
		}

		internal override bool ApplyChanges()
		{
			if(!m_pd.IsOpen) { Debug.Assert(false); return false; }

			MultiApplyStrings();
			MultiApplyIcon();
			MultiApplyProperties();
			MultiApplyCustomData();

			bool bModAny = false;
			for(int i = 0; i < m_v.Length; ++i)
			{
				if(m_vModified[i])
				{
					m_v[i].Touch(true, false);

					m_vModified[i] = false;
					bModAny = true;
				}
			}

			return bModAny;
		}

		private void PrepareMod(int iEntry)
		{
			if(m_vModified[iEntry]) return;
			m_vModified[iEntry] = true;

			m_v[iEntry].CreateBackup(m_pd);
		}

		private void EnsureStandardStrings()
		{
			foreach(string strKey in PwDefs.GetStandardFields())
			{
				if(!m_peM.Strings.Exists(strKey))
					m_peM.Strings.Set(strKey, (m_pd.MemoryProtection.GetProtection(
						strKey) ? ProtectedString.EmptyEx : ProtectedString.Empty));

				// Standard string protections are normalized while
				// loading/saving the database file
				this.MultiStringProt[strKey] = true;
			}
		}

		private void MultiInitStrings()
		{
			ProtectedString psCue = MultipleValuesEx.CueProtectedString;
			ProtectedString psCueEx = MultipleValuesEx.CueProtectedStringEx;

			EnsureStandardStrings();

			for(int i = 0; i < m_v.Length; ++i)
			{
				PwEntry pe = m_v[i];

				foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
				{
					if(i == 0) m_peM.Strings.Set(kvp.Key, kvp.Value);
					else
					{
						bool bProt = kvp.Value.IsProtected;
						ProtectedString psM = m_peM.Strings.Get(kvp.Key);

						if(psM == null)
							m_peM.Strings.Set(kvp.Key, (bProt ? psCueEx : psCue));
						else
						{
							bool bProtM = psM.IsProtected;

							if(!psM.Equals(kvp.Value, false))
								m_peM.Strings.Set(kvp.Key, ((bProtM && bProt) ?
									psCueEx : psCue));

							if(bProtM != bProt)
							{
								this.MultiStringProt[kvp.Key] = true;
								if(bProtM)
									m_peM.Strings.EnableProtection(kvp.Key,
										false); // May be set to cue above
							}
						}
					}
				}

				List<KeyValuePair<string, ProtectedString>> lM =
					new List<KeyValuePair<string, ProtectedString>>(m_peM.Strings);
				foreach(KeyValuePair<string, ProtectedString> kvpM in lM)
				{
					if(pe.Strings.Exists(kvpM.Key)) continue;

					if(!PwDefs.IsStandardField(kvpM.Key) || !kvpM.Value.IsEmpty)
						m_peM.Strings.Set(kvpM.Key, (kvpM.Value.IsProtected ?
							psCueEx : psCue));
				}
			}
		}

		private void MultiApplyStrings()
		{
			ProtectedString psCue = MultipleValuesEx.CueProtectedString;

			EnsureStandardStrings();

			for(int i = 0; i < m_v.Length; ++i)
			{
				PwEntry pe = m_v[i];

				foreach(KeyValuePair<string, ProtectedString> kvpM in m_peM.Strings)
				{
					ProtectedString ps = pe.Strings.Get(kvpM.Key);

					bool bProt = kvpM.Value.IsProtected;
					bool bMultiProt;
					this.MultiStringProt.TryGetValue(kvpM.Key, out bMultiProt);
					if(bMultiProt && (ps != null))
						bProt = ps.IsProtected;

					if(kvpM.Value.Equals(psCue, false))
					{
						if((ps != null) && (ps.IsProtected != bProt))
						{
							PrepareMod(i);
							pe.Strings.Set(kvpM.Key, ps.WithProtection(bProt));
						}
					}
					else if(kvpM.Value.IsEmpty && (ps == null) &&
						PwDefs.IsStandardField(kvpM.Key))
					{
						// Do not create the string
					}
					else
					{
						if((ps == null) || !ps.Equals(kvpM.Value, false) ||
							(ps.IsProtected != bProt))
						{
							PrepareMod(i);
							pe.Strings.Set(kvpM.Key, kvpM.Value.WithProtection(bProt));
						}
					}
				}

				List<string> lKeys = pe.Strings.GetKeys();
				foreach(string strKey in lKeys)
				{
					if(!m_peM.Strings.Exists(strKey))
					{
						PrepareMod(i);
						pe.Strings.Remove(strKey);
					}
				}
			}
		}

		private void MultiInitIcon()
		{
			m_peM.IconId = m_v[0].IconId;
			m_peM.CustomIconUuid = m_v[0].CustomIconUuid;

			bool bCustomZ = m_peM.CustomIconUuid.IsZero;

			for(int i = 1; i < m_v.Length; ++i)
			{
				PwEntry pe = m_v[i];

				bool bCustomE = pe.CustomIconUuid.Equals(m_peM.CustomIconUuid);
				if(!bCustomE || (bCustomZ && (pe.IconId != m_peM.IconId)))
				{
					m_peM.IconId = PwIcon.Key;
					m_peM.CustomIconUuid = m_puCueIcon;
					break;
				}
			}
		}

		private void MultiApplyIcon()
		{
			if(m_peM.CustomIconUuid.Equals(m_puCueIcon)) return;

			bool bCustomZ = m_peM.CustomIconUuid.IsZero;

			for(int i = 0; i < m_v.Length; ++i)
			{
				PwEntry pe = m_v[i];

				bool bCustomE = pe.CustomIconUuid.Equals(m_peM.CustomIconUuid);
				if(!bCustomE)
				{
					PrepareMod(i);
					pe.CustomIconUuid = m_peM.CustomIconUuid;

					if(bCustomZ) pe.IconId = m_peM.IconId;
				}
				else if(bCustomZ && (pe.IconId != m_peM.IconId))
				{
					PrepareMod(i);
					pe.IconId = m_peM.IconId;
				}
			}
		}

		private void MultiInitProperties()
		{
			string strCue = MultipleValuesEx.CueString;

			bool bExpM = m_v[0].Expires;
			m_peM.Expires = bExpM;
			m_peM.ExpiryTime = m_v[0].ExpiryTime;

			m_peM.ForegroundColor = m_v[0].ForegroundColor;
			m_peM.BackgroundColor = m_v[0].BackgroundColor;

			bool bTagsEq = true;
			m_peM.Tags = new List<string>(m_v[0].Tags);

			m_peM.OverrideUrl = m_v[0].OverrideUrl;

			m_peM.AutoType.Enabled = m_v[0].AutoType.Enabled;
			m_peM.AutoType.DefaultSequence = m_v[0].AutoType.DefaultSequence;
			m_peM.AutoType.ObfuscationOptions = m_v[0].AutoType.ObfuscationOptions;

			for(int i = 1; i < m_v.Length; ++i)
			{
				PwEntry pe = m_v[i];

				if((pe.Expires != bExpM) || (bExpM && (pe.ExpiryTime != m_peM.ExpiryTime)))
				{
					this.MultiExpiry = true;
					m_peM.Expires = false;
					bExpM = false;
				}

				if(!UIUtil.ColorsEqual(pe.ForegroundColor, m_peM.ForegroundColor))
				{
					this.MultiFgColor = true;
					m_peM.ForegroundColor = Color.Empty;
				}

				if(!UIUtil.ColorsEqual(pe.BackgroundColor, m_peM.BackgroundColor))
				{
					this.MultiBgColor = true;
					m_peM.BackgroundColor = Color.Empty;
				}

				if(bTagsEq && !MemUtil.ListsEqual<string>(pe.Tags, m_peM.Tags))
				{
					m_peM.Tags.Clear(); // We own it, see above
					m_peM.Tags.Add(strCue);
					bTagsEq = false;
				}

				if(pe.OverrideUrl != m_peM.OverrideUrl)
					m_peM.OverrideUrl = strCue;

				if(pe.AutoType.Enabled != m_peM.AutoType.Enabled)
				{
					this.MultiAutoTypeEnabled = true;
					m_peM.AutoType.Enabled = true;
				}

				if(pe.AutoType.DefaultSequence != m_peM.AutoType.DefaultSequence)
					m_peM.AutoType.DefaultSequence = strCue;

				if(pe.AutoType.ObfuscationOptions != m_peM.AutoType.ObfuscationOptions)
				{
					this.MultiAutoTypeObf = true;
					m_peM.AutoType.ObfuscationOptions = AutoTypeObfuscationOptions.None;
				}
			}
		}

		private void MultiApplyProperties()
		{
			string strCue = MultipleValuesEx.CueString;

			bool bExpM = m_peM.Expires;

			bool bSetTags = ((m_peM.Tags.Count != 1) || (m_peM.Tags[0] != strCue));
			bool bSetOvUrl = (m_peM.OverrideUrl != strCue);
			bool bSetAtSeq = (m_peM.AutoType.DefaultSequence != strCue);

			for(int i = 0; i < m_v.Length; ++i)
			{
				PwEntry pe = m_v[i];

				if(!this.MultiExpiry && ((pe.Expires != bExpM) ||
					(bExpM && (pe.ExpiryTime != m_peM.ExpiryTime))))
				{
					PrepareMod(i);
					pe.Expires = bExpM;

					if(bExpM) pe.ExpiryTime = m_peM.ExpiryTime;
				}

				if(!this.MultiFgColor && !UIUtil.ColorsEqual(pe.ForegroundColor,
					m_peM.ForegroundColor))
				{
					PrepareMod(i);
					pe.ForegroundColor = m_peM.ForegroundColor;
				}

				if(!this.MultiBgColor && !UIUtil.ColorsEqual(pe.BackgroundColor,
					m_peM.BackgroundColor))
				{
					PrepareMod(i);
					pe.BackgroundColor = m_peM.BackgroundColor;
				}

				if(bSetTags && !MemUtil.ListsEqual<string>(pe.Tags, m_peM.Tags))
				{
					PrepareMod(i);
					pe.Tags = new List<string>(m_peM.Tags);
				}

				if(bSetOvUrl && (pe.OverrideUrl != m_peM.OverrideUrl))
				{
					PrepareMod(i);
					pe.OverrideUrl = m_peM.OverrideUrl;
				}

				if(!this.MultiAutoTypeEnabled && (pe.AutoType.Enabled !=
					m_peM.AutoType.Enabled))
				{
					PrepareMod(i);
					pe.AutoType.Enabled = m_peM.AutoType.Enabled;
				}

				if(bSetAtSeq && (pe.AutoType.DefaultSequence !=
					m_peM.AutoType.DefaultSequence))
				{
					PrepareMod(i);
					pe.AutoType.DefaultSequence = m_peM.AutoType.DefaultSequence;
				}

				if(!this.MultiAutoTypeObf && (pe.AutoType.ObfuscationOptions !=
					m_peM.AutoType.ObfuscationOptions))
				{
					PrepareMod(i);
					pe.AutoType.ObfuscationOptions = m_peM.AutoType.ObfuscationOptions;
				}
			}
		}

		private void MultiInitCustomData()
		{
			string strCue = MultipleValuesEx.CueString;

			for(int i = 0; i < m_v.Length; ++i)
			{
				PwEntry pe = m_v[i];

				foreach(KeyValuePair<string, string> kvp in pe.CustomData)
				{
					if(i == 0) m_peM.CustomData.Set(kvp.Key, kvp.Value);
					else
					{
						string strM = m_peM.CustomData.Get(kvp.Key);
						if((strM == null) || (strM != kvp.Value))
							m_peM.CustomData.Set(kvp.Key, strCue);
					}
				}

				List<KeyValuePair<string, string>> lM =
					new List<KeyValuePair<string, string>>(m_peM.CustomData);
				foreach(KeyValuePair<string, string> kvpM in lM)
				{
					if(!pe.CustomData.Exists(kvpM.Key))
						m_peM.CustomData.Set(kvpM.Key, strCue);
				}
			}
		}

		private void MultiApplyCustomData()
		{
			string strCue = MultipleValuesEx.CueString;

			for(int i = 0; i < m_v.Length; ++i)
			{
				PwEntry pe = m_v[i];

				foreach(KeyValuePair<string, string> kvpM in m_peM.CustomData)
				{
					if(kvpM.Value == strCue) continue;

					string str = pe.CustomData.Get(kvpM.Key);
					if((str == null) || (str != kvpM.Value))
					{
						PrepareMod(i);
						pe.CustomData.Set(kvpM.Key, kvpM.Value);
					}
				}

				List<KeyValuePair<string, string>> l =
					new List<KeyValuePair<string, string>>(pe.CustomData);
				foreach(KeyValuePair<string, string> kvp in l)
				{
					if(!m_peM.CustomData.Exists(kvp.Key))
					{
						PrepareMod(i);
						pe.CustomData.Remove(kvp.Key);
					}
				}
			}
		}
	}
}
