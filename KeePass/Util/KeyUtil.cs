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
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Forms;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Keys;
using KeePassLib.Resources;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class KeyUtil
	{
		internal static CompositeKey CreateKey(byte[] pbPasswordUtf8,
			string strKeyFile, bool bUserAccount, IOConnectionInfo ioc,
			bool bNewKey, bool bSecureDesktop)
		{
			Debug.Assert(ioc != null);

			CompositeKey ck = new CompositeKey();
			bool bPassword = (pbPasswordUtf8 != null);
			string strNP = MessageService.NewParagraph;

			if(bPassword)
				ck.AddUserKey(new KcpPassword(pbPasswordUtf8,
					Program.Config.Security.MasterPassword.RememberWhileOpen));

			if(!string.IsNullOrEmpty(strKeyFile) && (strKeyFile !=
				KPRes.NoKeyFileSpecifiedMeta))
			{
				KeyProvider kp = Program.KeyProviderPool.Get(strKeyFile);
				if(kp != null)
				{
					byte[] pbKey = null;
					try
					{
						if(bSecureDesktop && !kp.SecureDesktopCompatible)
							throw new Exception(KPRes.KeyProvIncmpWithSD + strNP +
								KPRes.KeyProvIncmpWithSDHint);
						if(kp.Exclusive && (bPassword || bUserAccount))
							throw new Exception(KPRes.KeyProvExclusive);

						KeyProviderQueryContext ctx = new KeyProviderQueryContext(
							ioc, bNewKey, bSecureDesktop);

						pbKey = kp.GetKey(ctx);

						if((pbKey != null) && (pbKey.Length != 0))
							ck.AddUserKey(new KcpCustomKey(strKeyFile, pbKey,
								!kp.DirectKey));
						else return null; // Provider has shown error message
					}
					catch(Exception ex)
					{
						throw new Exception(strKeyFile + strNP + ex.Message);
					}
					finally { if(pbKey != null) MemUtil.ZeroByteArray(pbKey); }
				}
				else // Key file
				{
					try { ck.AddUserKey(new KcpKeyFile(strKeyFile, bNewKey)); }
					catch(Exception ex)
					{
						throw new Exception(strKeyFile + strNP + KLRes.FileLoadFailed +
							strNP + ex.Message);
					}
				}
			}

			if(bUserAccount) ck.AddUserKey(new KcpUserAccount());

			return ((ck.UserKeyCount != 0) ? ck : null);
		}

		internal static CompositeKey KeyFromUI(CheckBox cbPassword,
			PwInputControlGroup icgPassword, SecureTextBoxEx stbPassword,
			CheckBox cbKeyFile, ComboBox cmbKeyFile, CheckBox cbUserAccount,
			IOConnectionInfo ioc, bool bSecureDesktop)
		{
			if(cbPassword == null) { Debug.Assert(false); return null; }
			if(stbPassword == null) { Debug.Assert(false); return null; }
			if(cbKeyFile == null) { Debug.Assert(false); return null; }
			if(cmbKeyFile == null) { Debug.Assert(false); return null; }
			if(cbUserAccount == null) { Debug.Assert(false); return null; }

			bool bNewKey = (icgPassword != null);
			byte[] pbPasswordUtf8 = null;

			try
			{
				if(cbPassword.Checked)
				{
					pbPasswordUtf8 = stbPassword.TextEx.ReadUtf8();

					if(bNewKey)
					{
						if(!icgPassword.ValidateData(true)) return null;

						string strError = ValidateNewMasterPassword(pbPasswordUtf8,
							(uint)stbPassword.TextLength);
						if(strError != null)
						{
							if(strError.Length != 0) MessageService.ShowWarning(strError);
							return null;
						}
					}
				}

				string strKeyFile = null;
				if(cbKeyFile.Checked) strKeyFile = cmbKeyFile.Text;

				return CreateKey(pbPasswordUtf8, strKeyFile, cbUserAccount.Checked,
					ioc, bNewKey, bSecureDesktop);
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
			finally
			{
				if(pbPasswordUtf8 != null) MemUtil.ZeroByteArray(pbPasswordUtf8);
			}

			return null;
		}

		private static string ValidateNewMasterPassword(byte[] pbUtf8, uint cChars)
		{
			if(pbUtf8 == null) { Debug.Assert(false); throw new ArgumentNullException("pbUtf8"); }

#if DEBUG
			char[] vChars = StrUtil.Utf8.GetChars(pbUtf8);
			Debug.Assert(vChars.Length == (int)cChars);
			MemUtil.ZeroArray<char>(vChars);
#endif

			uint cBits = QualityEstimation.EstimatePasswordBits(pbUtf8);

			uint cMinChars = Program.Config.Security.MasterPassword.MinimumLength;
			if(cChars < cMinChars)
				return KPRes.MasterPasswordMinLengthFailed.Replace(@"{PARAM}",
					cMinChars.ToString());

			uint cMinBits = Program.Config.Security.MasterPassword.MinimumQuality;
			if(cBits < cMinBits)
				return KPRes.MasterPasswordMinQualityFailed.Replace(@"{PARAM}",
					cMinBits.ToString());

			string strError = Program.KeyValidatorPool.Validate(pbUtf8,
				KeyValidationType.MasterPassword);
			if(strError != null) return strError;

			if(cChars == 0)
			{
				if(!MessageService.AskYesNo(KPRes.EmptyMasterPw +
					MessageService.NewParagraph + KPRes.EmptyMasterPwHint +
					MessageService.NewParagraph + KPRes.EmptyMasterPwQuestion,
					null, false))
					return string.Empty;
			}

			if(cBits <= PwDefs.QualityBitsWeak)
			{
				string str = KPRes.MasterPasswordWeak + MessageService.NewParagraph +
					KPRes.MasterPasswordConfirm;
				if(!MessageService.AskYesNo(str, null, false, MessageBoxIcon.Warning))
					return string.Empty;
			}

			return null;
		}

		public static CompositeKey KeyFromCommandLine(CommandLineArgs args)
		{
			if(args == null) throw new ArgumentNullException("args");

			string strFile = args.FileName;
			IOConnectionInfo ioc = (!string.IsNullOrEmpty(strFile) ?
				IOConnectionInfo.FromPath(strFile) : new IOConnectionInfo());

			string strPassword = args[AppDefs.CommandLineOptions.Password];
			string strPasswordEnc = args[AppDefs.CommandLineOptions.PasswordEncrypted];
			string strPasswordStdIn = args[AppDefs.CommandLineOptions.PasswordStdIn];
			string strKeyFile = args[AppDefs.CommandLineOptions.KeyFile];
			string strUserAcc = args[AppDefs.CommandLineOptions.UserAccount];

			byte[] pbPasswordUtf8 = null;
			try
			{
				if(strPassword != null)
					pbPasswordUtf8 = StrUtil.Utf8.GetBytes(strPassword);
				else if(strPasswordEnc != null)
					pbPasswordUtf8 = StrUtil.Utf8.GetBytes(StrUtil.DecryptString(
						strPasswordEnc));
				else if(strPasswordStdIn != null)
				{
					ProtectedString ps = ReadPasswordStdIn(true);
					if(ps == null) return null;
					pbPasswordUtf8 = ps.ReadUtf8();
				}

				return CreateKey(pbPasswordUtf8, strKeyFile, (strUserAcc != null),
					ioc, false, false);
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
			finally
			{
				if(pbPasswordUtf8 != null) MemUtil.ZeroByteArray(pbPasswordUtf8);
				ClearKeyOptions(args, false, false);
			}

			return null;
		}

		internal static void ClearKeyOptions(CommandLineArgs args, bool bForce,
			bool bClearPreSelect)
		{
			if(args == null) { Debug.Assert(false); return; }

			if(!bForce && !Program.Config.Security.ClearKeyCommandLineParams)
				return;

			args.Remove(AppDefs.CommandLineOptions.Password);
			args.Remove(AppDefs.CommandLineOptions.PasswordEncrypted);
			args.Remove(AppDefs.CommandLineOptions.PasswordStdIn);
			args.Remove(AppDefs.CommandLineOptions.KeyFile);
			args.Remove(AppDefs.CommandLineOptions.UserAccount);

			if(bClearPreSelect)
				args.Remove(AppDefs.CommandLineOptions.PreSelect);
		}

		private static bool g_bReadPwStdIn = false;
		private static ProtectedString g_psReadPwStdIn = null;
		/// <summary>
		/// Read a password from StdIn. The password is read only once
		/// and then cached.
		/// </summary>
		internal static ProtectedString ReadPasswordStdIn(bool bFailWithUI)
		{
			if(g_bReadPwStdIn) return g_psReadPwStdIn;

			try
			{
				string str = Console.ReadLine();
				if(str != null)
					g_psReadPwStdIn = new ProtectedString(true, str.Trim());
			}
			catch(Exception ex)
			{
				if(bFailWithUI) MessageService.ShowWarning(ex);
			}

			g_bReadPwStdIn = true;
			return g_psReadPwStdIn;
		}

		internal static string[] MakeCtxIndependent(string[] vCmdLineArgs)
		{
			if(vCmdLineArgs == null) { Debug.Assert(false); return MemUtil.EmptyArray<string>(); }

			CommandLineArgs cl = new CommandLineArgs(vCmdLineArgs);
			List<string> lFlt = new List<string>();

			foreach(string strArg in vCmdLineArgs)
			{
				KeyValuePair<string, string> kvpArg = CommandLineArgs.GetParameter(strArg);
				if(kvpArg.Key.Equals(AppDefs.CommandLineOptions.PasswordStdIn, StrUtil.CaseIgnoreCmp))
				{
					ProtectedString ps = ReadPasswordStdIn(true);

					if((cl[AppDefs.CommandLineOptions.Password] == null) &&
						(cl[AppDefs.CommandLineOptions.PasswordEncrypted] == null) &&
						(ps != null))
					{
						lFlt.Add("-" + AppDefs.CommandLineOptions.Password + ":" +
							ps.ReadString()); // No quote wrapping/encoding
					}
				}
				else lFlt.Add(strArg);
			}

			return lFlt.ToArray();
		}

		public static bool ReAskKey(PwDatabase pd, bool bFailWithUI)
		{
			if(pd == null) { Debug.Assert(false); return false; }

			KeyPromptFormResult r;
			DialogResult dr = KeyPromptForm.ShowDialog(pd.IOConnectionInfo,
				false, KPRes.EnterCurrentCompositeKey, out r);
			if((dr != DialogResult.OK) || (r == null)) return false;

			CompositeKey ck = r.CompositeKey;
			bool bEqual = ck.EqualsValue(pd.MasterKey);

			if(!bEqual && bFailWithUI)
				MessageService.ShowWarning(KLRes.InvalidCompositeKey,
					KLRes.InvalidCompositeKeyHint);

			return bEqual;
		}
	}
}
