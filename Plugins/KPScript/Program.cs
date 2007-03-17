using System;
using System.Collections.Generic;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Util;

namespace KPScript
{
	static class Program
	{
		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			CommandLineArgs cmdArgs = new CommandLineArgs();
			cmdArgs.Parse(args);
			cmdArgs.Lock();

			if((cmdArgs.FileName != null) &&
				cmdArgs.FileName.EndsWith(AppDefs.ScriptExtension))
			{
				ScriptingUtil.RunScriptFile(cmdArgs.FileName);
				return;
			}

			// Application.Run(new MainForm());
		}
	}
}