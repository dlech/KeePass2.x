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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Windows.Forms;
using System.IO;

using Microsoft.CSharp;

using KeePass.Util;

using KeePassLib;
using KeePassLib.Utility;

namespace KPScript
{
	public static class KpsRunner
	{
		private const string CsUsing = "using System;\r\nusing System.IO;\r\n" +
			"using System.Collections;\r\nusing System.Collections.Generic;\r\n" +
			"using System.Text;\r\nusing System.Windows.Forms;\r\n" +
			"using KeePass.App;\r\nusing KeePass.Forms;\r\n" +
			"using KeePass.UI;\r\nusing KeePass.Util;\r\n" +
			"using KeePassLib;\r\nusing KeePassLib.Collections;\r\n" +
			"using KeePassLib.Cryptography;\r\nusing KeePassLib.Cryptography.Cipher;\r\n" +
			"using KeePassLib.Delegates;\r\nusing KeePassLib.Interfaces;\r\n" +
			"using KeePassLib.Keys;\r\nusing KeePassLib.Security;\r\n" +
			"using KeePassLib.Serialization;\r\nusing KeePassLib.Utility;\r\n";
		private const string CsClass = "namespace KeePass.Scripting {" +
			"public static class ThisScript {";
		private const string CsPost = "} }";

		public static void RunScriptFile(string strFile)
		{
			string strScript = File.ReadAllText(strFile);
			RunScript(strScript);
		}

		public static void RunScript(string strScript)
		{
			RunCSharpScript(strScript);
		}

		private static void RunCSharpScript(string strScript)
		{
			string[] vUsing = CsUsing.Split(new string[] { "\r\n" },
				StringSplitOptions.None);
			string[] vClass = CsClass.Split(new string[] { "\r\n" },
				StringSplitOptions.None);
			int nLineOffset = vUsing.Length + vClass.Length;

			string str = CsUsing + CsClass + strScript + CsPost;

			CSharpCodeProvider cscp = new CSharpCodeProvider();

			CompilerParameters cp = new CompilerParameters();
			cp.ReferencedAssemblies.Add("System.dll");
			cp.ReferencedAssemblies.Add("System.Data.dll");
			cp.ReferencedAssemblies.Add("System.Deployment.dll");
			cp.ReferencedAssemblies.Add("System.Drawing.dll");
			cp.ReferencedAssemblies.Add("System.Security.dll");
			cp.ReferencedAssemblies.Add("System.Windows.Forms.dll");
			cp.ReferencedAssemblies.Add("System.Xml.dll");
			cp.ReferencedAssemblies.Add(WinUtil.GetExecutable());
			cp.GenerateExecutable = false;
			cp.GenerateInMemory = true;
			cp.IncludeDebugInformation = false;
			cp.TreatWarningsAsErrors = true;
			cp.WarningLevel = 4;

			CompilerResults cr = cscp.CompileAssemblyFromSource(cp, str);

			if(cr.Errors.HasErrors)
			{
				StringBuilder sbErrors = new StringBuilder();
				foreach(CompilerError ce in cr.Errors)
					sbErrors.AppendLine((ce.Line - nLineOffset).ToString() +
						": " + ce.ErrorText);

				throw new Exception(sbErrors.ToString());
			}

			Assembly asm = cr.CompiledAssembly;

			Module m = asm.GetModules(false)[0];
			Type t = m.GetType("KeePass.Scripting.ThisScript");
			t.GetMethod("Main").Invoke(null, null);

			File.Delete(cp.OutputAssembly);
		}
	}
}
