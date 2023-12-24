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
using System.Xml;

using KeePass.App;
using KeePass.Forms;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.KeyDerivation;
using KeePassLib.Keys;
using KeePassLib.Native;
using KeePassLib.Resources;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class KeyUtil
	{
		private const string KdfPrcParams = "P";
		private const string KdfPrcTime = "T";
		private const string KdfPrcError = "E";

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
			return ReAskKey(pd, bFailWithUI, null);
		}

		internal static bool ReAskKey(PwDatabase pd, bool bFailWithUI,
			string strContext)
		{
			if(pd == null) { Debug.Assert(false); return false; }

			string strTitle = GetReAskKeyTitle(strContext);

			KeyPromptFormResult r;
			DialogResult dr = KeyPromptForm.ShowDialog(pd.IOConnectionInfo,
				false, strTitle, out r);
			if((dr != DialogResult.OK) || (r == null)) return false;

			CompositeKey ck = r.CompositeKey;
			bool bEqual = ck.EqualsValue(pd.MasterKey);

			if(!bEqual && bFailWithUI)
				MessageService.ShowWarning(KLRes.InvalidCompositeKey,
					KLRes.InvalidCompositeKeyHint);

			return bEqual;
		}

		internal static string GetReAskKeyTitle(string strContext)
		{
			string str = KPRes.EnterCurrentCompositeKey;

			if(!string.IsNullOrEmpty(strContext))
				str += " (" + strContext + ")";

			return str;
		}

		// Returns false if the child process cannot be spawned.
		// Throws an exception if an exception occurs in the child process
		// during the KDF computation.
		internal static bool KdfPrcTest(KdfParameters p, out ulong uTimeMS)
		{
			uTimeMS = 0;
			if(p == null) { Debug.Assert(false); return false; }

			Process prc = null;
			string strError = null;
			try
			{
				VariantDictionary d = new VariantDictionary();
				d.SetByteArray(KdfPrcParams, KdfParameters.SerializeExt(p));

				string strInfoFile = Program.TempFilesPool.GetTempFileName(true);
				File.WriteAllBytes(strInfoFile, VariantDictionary.Serialize(d));

				string strArgs = "-" + AppDefs.CommandLineOptions.KdfTest +
					":\"" + NativeLib.EncodeDataToArgs(strInfoFile) + "\"";

				prc = WinUtil.StartSelfEx(strArgs);
				if(prc == null) { Debug.Assert(false); return false; }
				prc.WaitForExit();

				d = VariantDictionary.Deserialize(File.ReadAllBytes(strInfoFile));

				strError = d.GetString(KdfPrcError);
				if(string.IsNullOrEmpty(strError))
				{
					uTimeMS = d.GetUInt64(KdfPrcTime, ulong.MaxValue);
					if(uTimeMS != ulong.MaxValue) return true;
					else { Debug.Assert(false); }
				}
			}
			catch(ThreadAbortException) { }
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				if(prc != null)
				{
					try { if(!prc.HasExited) prc.Kill(); }
					catch(Exception) { Debug.Assert(false); }
					try { prc.Dispose(); }
					catch(Exception) { Debug.Assert(false); }
				}
			}

			if(!string.IsNullOrEmpty(strError)) throw new Exception(strError);
			return false;
		}

		// Returns true if and only if the command line parameter is present
		internal static bool KdfPrcTestAsChild()
		{
			string strInfoFile = Program.CommandLineArgs[AppDefs.CommandLineOptions.KdfTest];
			if(string.IsNullOrEmpty(strInfoFile)) return false;

			VariantDictionary d = null;
			try
			{
				d = VariantDictionary.Deserialize(File.ReadAllBytes(strInfoFile));

				byte[] pb = d.GetByteArray(KdfPrcParams);
				if((pb == null) || (pb.Length == 0)) { Debug.Assert(false); return true; }

				KdfParameters p = KdfParameters.DeserializeExt(pb);
				if(p == null) { Debug.Assert(false); return true; }

				KdfEngine kdf = KdfPool.Get(p.KdfUuid);
				if(kdf == null) { Debug.Assert(false); return true; }

				d.SetUInt64(KdfPrcTime, kdf.Test(p));
			}
			catch(Exception ex)
			{
				Debug.Assert(false);
				if((d != null) && !string.IsNullOrEmpty(ex.Message))
					d.SetString(KdfPrcError, ex.Message);
			}

			try
			{
				if(d != null)
					File.WriteAllBytes(strInfoFile, VariantDictionary.Serialize(d));
			}
			catch(Exception) { Debug.Assert(false); }
			return true;
		}

		internal static bool KdfAdjustWeakParameters(ref KdfParameters p,
			IOConnectionInfo iocDatabase)
		{
			if(p == null) { Debug.Assert(false); return false; }

			if(!Program.Config.Security.KeyTransformWeakWarning) return false;

			KdfEngine kdf = KdfPool.Get(p.KdfUuid);
			if(kdf == null) { Debug.Assert(false); return false; }
			if(!kdf.AreParametersWeak(p)) return false;

			string strMsg = KPRes.KeyTransformWeak + MessageService.NewParagraph +
				KPRes.KeyTransformDefaultsQ;
			string strPath = ((iocDatabase != null) ? iocDatabase.GetDisplayName() : null);
			if(!string.IsNullOrEmpty(strPath))
				strMsg = strPath + MessageService.NewParagraph + strMsg;

			VistaTaskDialog dlg = new VistaTaskDialog();
			dlg.Content = strMsg;
			dlg.DefaultButtonID = (int)DialogResult.OK;
			dlg.EnableHyperlinks = true;
			dlg.FooterText = VistaTaskDialog.CreateLink("h", KPRes.MoreInfo);
			dlg.VerificationText = UIUtil.GetDialogNoShowAgainText(KPRes.No);
			dlg.WindowTitle = PwDefs.ShortProductName;
			dlg.SetIcon(VtdCustomIcon.Question);
			dlg.SetFooterIcon(VtdIcon.Information);
			dlg.AddButton((int)DialogResult.OK, KPRes.YesCmd, null);
			dlg.AddButton((int)DialogResult.Cancel, KPRes.NoCmd, null);

			dlg.LinkClicked += delegate(object sender, LinkClickedEventArgs e)
			{
				string str = ((e != null) ? e.LinkText : null);
				if(string.Equals(str, "h", StrUtil.CaseIgnoreCmp))
					AppHelp.ShowHelp(AppDefs.HelpTopics.Security,
						AppDefs.HelpTopics.SecurityDictProt);
				else { Debug.Assert(false); }
			};

			int dr;
			if(dlg.ShowDialog())
			{
				dr = dlg.Result;
				if(dlg.ResultVerificationChecked)
					Program.Config.Security.KeyTransformWeakWarning = false;
			}
			else
				dr = (MessageService.AskYesNo(strMsg) ? (int)DialogResult.OK :
					(int)DialogResult.Cancel);

			if(dr == (int)DialogResult.OK)
			{
				p = kdf.GetDefaultParameters();
				return true;
			}
			return false;
		}

		internal static bool HasKeyExpired(PwDatabase pd, string strExpiry,
			string strDesc)
		{
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return false; }

			strExpiry = (strExpiry ?? string.Empty).Trim();
			if(strExpiry.Length == 0) return false;

			try
			{
				DateTime dtChanged = pd.MasterKeyChanged;

				if(strExpiry.StartsWith("P", StrUtil.CaseIgnoreCmp) ||
					strExpiry.StartsWith("-P", StrUtil.CaseIgnoreCmp))
				{
					TimeSpan ts = XmlConvert.ToTimeSpan(strExpiry);
					return ((dtChanged + ts) < DateTime.UtcNow);
				}

				DateTime dt = XmlConvert.ToDateTime(strExpiry,
					XmlDateTimeSerializationMode.Utc);
				return (dtChanged < dt);
			}
			catch(Exception ex) { MessageService.ShowWarning(strDesc, ex); }

			return false;
		}
	}
}
