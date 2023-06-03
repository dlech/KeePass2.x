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
using System.Text;

using KeePass.App.Configuration;
using KeePass.Resources;

#if !KeePassUAP
using KeePass.Util.Spr;
#endif

using KeePassLib;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class PwGeneratorUtil
	{
		private static string g_strBuiltInSuffix = null;
		internal static string BuiltInSuffix
		{
			get
			{
				if(g_strBuiltInSuffix == null)
					g_strBuiltInSuffix = " (" + KPRes.BuiltIn + ")";
				return g_strBuiltInSuffix;
			}
		}

		private static List<PwProfile> g_lBuiltIn = null;
		public static List<PwProfile> BuiltInProfiles
		{
			get
			{
				if(g_lBuiltIn == null) g_lBuiltIn = AllocStdProfiles();
				return g_lBuiltIn;
			}
		}

		private static List<PwProfile> AllocStdProfiles()
		{
			List<PwProfile> l = new List<PwProfile>();

			string strHex = KPRes.HexKeyEx;
			AddStdPattern(l, strHex.Replace(@"{PARAM}", "40"), @"H{10}");
			AddStdPattern(l, strHex.Replace(@"{PARAM}", "128"), @"H{32}");
			AddStdPattern(l, strHex.Replace(@"{PARAM}", "256"), @"H{64}");

			AddStdPattern(l, KPRes.MacAddress, "HH\\-HH\\-HH\\-HH\\-HH\\-HH");

			return l;
		}

		private static void AddStdPattern(List<PwProfile> l, string strName,
			string strPattern)
		{
			PwProfile prf = new PwProfile();

			prf.Name = strName + PwGeneratorUtil.BuiltInSuffix;
			prf.GeneratorType = PasswordGeneratorType.Pattern;
			prf.Pattern = strPattern;
			prf.CollectUserEntropy = false;

			l.Add(prf);
		}

		/// <summary>
		/// Get a list of all password generator profiles (built-in
		/// and user-defined ones).
		/// </summary>
		public static List<PwProfile> GetAllProfiles(bool bSort)
		{
			List<PwProfile> lUser = Program.Config.PasswordGenerator.UserProfiles;

			// Sort it in the configuration file
			if(bSort) SortProfiles(lUser);

			// Remove old built-in profiles by KeePass <= 2.17
			for(int i = lUser.Count - 1; i >= 0; --i)
			{
				if(IsBuiltInProfile(lUser[i].Name)) lUser.RemoveAt(i);
			}

			List<PwProfile> l = new List<PwProfile>();
			l.AddRange(PwGeneratorUtil.BuiltInProfiles);
			l.AddRange(lUser);
			if(bSort) SortProfiles(l);
			return l;
		}

		public static bool IsBuiltInProfile(string strName)
		{
			if(strName == null) { Debug.Assert(false); return false; }

			string strNameS = strName + PwGeneratorUtil.BuiltInSuffix;

			foreach(PwProfile prf in PwGeneratorUtil.BuiltInProfiles)
			{
				if(prf.Name.Equals(strName, StrUtil.CaseIgnoreCmp) ||
					prf.Name.Equals(strNameS, StrUtil.CaseIgnoreCmp))
					return true;
			}

			return false;
		}

		public static int CompareProfilesByName(PwProfile a, PwProfile b)
		{
			if(a == b) return 0;
			if(a == null) { Debug.Assert(false); return -1; }
			if(b == null) { Debug.Assert(false); return 1; }

			return StrUtil.CompareNaturally(a.Name, b.Name);
		}

		internal static void SortProfiles(List<PwProfile> l)
		{
			if(l == null) { Debug.Assert(false); return; }

			l.Sort(PwGeneratorUtil.CompareProfilesByName);
		}

		internal static int FindProfile(List<PwProfile> l, string strName,
			bool bIgnoreCase)
		{
			if(l == null) { Debug.Assert(false); return -1; }

			StringComparison sc = (bIgnoreCase ? StrUtil.CaseIgnoreCmp :
				StringComparison.Ordinal);

			for(int i = 0; i < l.Count; ++i)
			{
				if(l[i].Name.Equals(strName, sc)) return i;
			}

			return -1;
		}

#if !KeePassUAP
		internal static ProtectedString GenerateAcceptable(PwProfile prf,
			byte[] pbUserEntropy, PwEntry peOptCtx, PwDatabase pdOptCtx,
			bool bShowErrorUI)
		{
			bool bAcceptAlways = false;
			string strError;
			return GenerateAcceptable(prf, pbUserEntropy, peOptCtx, pdOptCtx,
				bShowErrorUI, ref bAcceptAlways, out strError);
		}

		internal static ProtectedString GenerateAcceptable(PwProfile prf,
			byte[] pbUserEntropy, PwEntry peOptCtx, PwDatabase pdOptCtx,
			bool bShowErrorUI, ref bool bAcceptAlways, out string strError)
		{
			strError = null;

			ProtectedString ps = ProtectedString.Empty;
			SprContext ctx = new SprContext(peOptCtx, pdOptCtx,
				SprCompileFlags.NonActive, false, false);

			while(true)
			{
				try
				{
					PwgError e = PwGenerator.Generate(out ps, prf, pbUserEntropy,
						Program.PwGeneratorPool);

					if(e != PwgError.Success)
					{
						strError = PwGenerator.ErrorToString(e, true);
						break;
					}
				}
				catch(Exception ex)
				{
					strError = PwGenerator.ErrorToString(ex, true);
					break;
				}
				finally
				{
					if(ps == null) { Debug.Assert(false); ps = ProtectedString.Empty; }
				}

				if(bAcceptAlways) break;

				string str = ps.ReadString();
				string strCmp = SprEngine.Compile(str, ctx);

				if(str != strCmp)
				{
					if(prf.GeneratorType == PasswordGeneratorType.CharSet)
						continue; // Silently try again

					string strText = str + MessageService.NewParagraph +
						KPRes.GenPwSprVariant + MessageService.NewParagraph +
						KPRes.GenPwAccept;

					if(!MessageService.AskYesNo(strText, null, false))
						continue;
					bAcceptAlways = true;
				}

				break;
			}

			if(!string.IsNullOrEmpty(strError))
			{
				ps = ProtectedString.Empty;
				if(bShowErrorUI) MessageService.ShowWarning(strError);
			}

			return ps;
		}

		internal static void GenerateAuto(PwEntry pe, PwDatabase pd)
		{
			if((pe == null) || (pd == null)) { Debug.Assert(false); return; }

			AcePasswordGenerator acePG = Program.Config.PasswordGenerator;
			PwProfile prf = (acePG.ProfilesEnabled ?
				acePG.AutoGeneratedPasswordsProfile : new PwProfile());

			ProtectedString ps = GenerateAcceptable(prf, null, pe, pd, false);

			pe.Strings.Set(PwDefs.PasswordField, ps.WithProtection(
				pd.MemoryProtection.ProtectPassword));
		}
#endif
	}
}
