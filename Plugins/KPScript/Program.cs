/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Forms;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Keys;
using KeePassLib.Serialization;

using KPScript.ScriptingModules;

namespace KPScript
{
	public static class Program
	{
		private const int ReturnCodeSuccess = 0;
		private const int ReturnCodeError = 1;

		private const string ScriptFileSuffix = "kps";

		private const string ParamCommand = "c";

		private const string ParamGuiKey = "guikeyprompt";
		private const string ParamConsoleKey = "keyprompt";

		private static bool m_bGuiInitialized = false;

		public static int Main(string[] args)
		{
			if((args == null) || (args.Length == 0))
			{
				PrintUsage();
				return ReturnCodeSuccess;
			}

			CryptoRandom.Initialize();

			CommandLineArgs cmdArgs = new CommandLineArgs(args);

			int nReturnCode = ReturnCodeSuccess;

			try
			{
				if((cmdArgs.FileName != null) &&
					(cmdArgs.FileName.EndsWith(ScriptFileSuffix)))
				{
					EnsureGuiInitialized();
					KpsRunner.RunScriptFile(cmdArgs.FileName);
				}
				else RunScriptLine(cmdArgs);

				WriteLineColored(@"OK: " + KSRes.OperationSuccessful,
					ConsoleColor.Green);
			}
			catch(Exception exScript)
			{
				if((exScript.Message != null) && (exScript.Message.Length > 0))
					WriteLineColored(@"E: " + exScript.Message,
						ConsoleColor.Red);
				else
					WriteLineColored(@"E: " + KSRes.UnknownException,
						ConsoleColor.Red);

				nReturnCode = ReturnCodeError;
			}

			return nReturnCode;
		}

		private static void PrintUsage()
		{
			Console.WriteLine("KPScript - Scripting Plugin");
			Console.WriteLine("Copyright © 2007 Dominik Reichl");
			Console.WriteLine();
			Console.WriteLine(PwDefs.ShortProductName + " Runtime: " +
				PwDefs.VersionString);
		}

		private static void EnsureGuiInitialized()
		{
			if(m_bGuiInitialized == false)
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				m_bGuiInitialized = true;
			}
		}

		internal static void WriteLineColored(string strText, ConsoleColor clr)
		{
			ConsoleColor clrPrevFg = Console.ForegroundColor;
			ConsoleColor clrPrevBg = Console.BackgroundColor;
			
			if(clr != clrPrevFg) Console.ForegroundColor = clr;
			if(clrPrevBg == clr)
			{
				if(clrPrevBg == ConsoleColor.Black)
					Console.BackgroundColor = ConsoleColor.Gray;
				else Console.BackgroundColor = ConsoleColor.Black;
			}

			Console.WriteLine(strText);

			Console.BackgroundColor = clrPrevBg;
			Console.ForegroundColor = clrPrevFg;
		}

		private static CompositeKey KeyFromCmdLine(CommandLineArgs args)
		{
			CompositeKey cmpKey = new CompositeKey();

			string strPw = args[AppDefs.CommandLineOptions.Password];
			if(strPw != null)
				cmpKey.AddUserKey(new KcpPassword(strPw));

			string strFile = args[AppDefs.CommandLineOptions.KeyFile];
			if(strFile != null)
				cmpKey.AddUserKey(new KcpKeyFile(strFile));

			string strUserAcc = args[AppDefs.CommandLineOptions.UserAccount];
			if(strUserAcc != null)
				cmpKey.AddUserKey(new KcpUserAccount());

			return cmpKey;
		}

		private static void RunScriptLine(CommandLineArgs args)
		{
			string strCommand = args[ParamCommand];
			if(strCommand == null)
				throw new InvalidOperationException(KSRes.NoCommand);

			if(args.FileName == null)
			{
				RunSingleCommand(strCommand.ToLower());
				return;
			}

			IOConnectionInfo ioc = new IOConnectionInfo();
			ioc.Url = args.FileName;
			ioc.CredSaveMode = IOCredSaveMode.NoSave;

			CompositeKey cmpKey = null;
			if(args[ParamGuiKey] != null)
			{
				EnsureGuiInitialized();
				KeyPromptForm kpf = new KeyPromptForm();
				kpf.InitEx(ioc.GetDisplayName());
				if(kpf.ShowDialog() != DialogResult.OK) return;

				cmpKey = kpf.CompositeKey;
			}
			else if(args[ParamConsoleKey] != null)
			{
				cmpKey = new CompositeKey();

				Console.WriteLine(KSRes.NoKeyPartHint);
				Console.WriteLine();
				Console.WriteLine(KSRes.KeyPrompt);

				Console.Write(KSRes.PasswordPrompt + " ");
				string strPw = Console.ReadLine().Trim();
				if((strPw != null) && (strPw.Length > 0))
					cmpKey.AddUserKey(new KcpPassword(strPw));

				Console.Write(KSRes.KeyFilePrompt + " ");
				string strFile = Console.ReadLine().Trim();
				if((strFile != null) && (strFile.Length > 0))
					cmpKey.AddUserKey(new KcpKeyFile(strFile));

				Console.Write(KSRes.UserAccountPrompt + " ");
				string strUA = Console.ReadLine().Trim();
				if(strUA != null)
				{
					string strUal = strUA.ToLower();
					if((strUal == "y") || (strUal == "j") ||
						(strUal == "o") || (strUal == "a") ||
						(strUal == "u"))
					{
						cmpKey.AddUserKey(new KcpUserAccount());
					}
				}
			}
			else cmpKey = KeyFromCmdLine(args);

			PwDatabase pwDb = new PwDatabase();
			pwDb.Open(ioc, cmpKey, null);

			bool bNeedsSave;
			RunFileCommand(strCommand.ToLower(), args, pwDb, out bNeedsSave);

			if(bNeedsSave) pwDb.Save(null);

			pwDb.Close();
		}

		private static void RunSingleCommand(string strCommand)
		{
			throw new Exception(KSRes.UnknownCommand);
		}

		private static void RunFileCommand(string strCommand, CommandLineArgs args,
			PwDatabase pwDb, out bool bNeedsSave)
		{
			bNeedsSave = false;

			if(ReportingMod.ProcessCommand(strCommand, args, pwDb, out bNeedsSave))
				return;
			else if(EditEntryMod.ProcessCommand(strCommand, args, pwDb, out bNeedsSave))
				return;

			throw new Exception(KSRes.UnknownCommand);
		}
	}
}
