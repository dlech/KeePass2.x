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
using System.Xml;

using KeePass.Util;

using KeePassLib.Utility;

using TrlUtil.App;
using TrlUtil.App.Configuration;
using TrlUtil.Native;

namespace TrlUtil
{
	public static class Program
	{
		private static TceConfig m_cfg = null;
		public static TceConfig Config
		{
			get
			{
				if(m_cfg == null) { Debug.Assert(false); m_cfg = new TceConfig(); }
				return m_cfg;
			}
		}

		[STAThread]
		public static void Main(string[] args)
		{
			try
			{
				ConfigureDpi();
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				KeePass.Program.EnableTranslation = false; // We need English
				KeePass.Program.CommonInitialize();

				m_cfg = (TceConfig.Load() ?? new TceConfig());

				MainPriv(args);
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, TuDefs.ProductName,
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			TceConfig.Save(m_cfg);

			try { KeePass.Program.CommonTerminate(); }
			catch(Exception) { Debug.Assert(false); }
		}

		private static void MainPriv(string[] args)
		{
			if((args != null) && (args.Length == 2))
			{
				ExecuteCmd(args[0], args[1]);
				return;
			}

			Application.Run(new MainForm());
		}

		private static void ConfigureDpi()
		{
			// Ensure that base hashes are computed with respect to 100% DPI
			try
			{
				if(WinUtil.IsAtLeastWindows10) // 8.1 partially
				{
					if(NativeMethods.SetProcessDpiAwareness(
						NativeMethods.ProcessDpiAwareness.Unaware) < 0)
					{
						Debug.Assert(false);
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static void ExecuteCmd(string strCmd, string strFile)
		{
			/* if(strCmd == "convert_resx")
			{
				XmlDocument xdIn = XmlUtilEx.CreateXmlDocument();
				xdIn.Load(strFile);

				using(StreamWriter sw = new StreamWriter(strFile + ".lng.xml",
					false, StrUtil.Utf8))
				{
					foreach(XmlNode xn in xdIn.DocumentElement.ChildNodes)
					{
						if(xn.Name != "data") continue;

						sw.Write("<Data Name=\"" + xn.Attributes["name"].Value +
							"\">\r\n\t<Value>" + xn.SelectSingleNode("value").InnerXml +
							"</Value>\r\n</Data>\r\n");
					}
				}
			} */
			/* else if(strCmd == "compress")
			{
				byte[] pbData = MemUtil.Compress(File.ReadAllBytes(strFile));
				File.WriteAllBytes(strFile + ".lngx", pbData);
			} */
			if(strCmd == "src_from_xml")
			{
				XmlDocument xdIn = XmlUtilEx.CreateXmlDocument();
				xdIn.Load(strFile);

				foreach(XmlNode xnTable in xdIn.DocumentElement.SelectNodes("StringTable"))
				{
					using(StreamWriter sw = new StreamWriter(xnTable.Attributes[
						"Name"].Value + ".Generated.cs", false, StrUtil.Utf8))
					{
						sw.WriteLine("// This is a generated file!");
						sw.WriteLine("// Do not edit manually, changes will be overwritten.");
						sw.WriteLine();
						sw.WriteLine("using System;");
						sw.WriteLine("using System.Collections.Generic;");
						sw.WriteLine();
						sw.WriteLine("namespace " + xnTable.Attributes["Namespace"].Value);
						sw.WriteLine("{");
						sw.WriteLine("\t/// <summary>");
						sw.WriteLine("\t/// A strongly-typed resource class, for looking up localized strings, etc.");
						sw.WriteLine("\t/// </summary>");
						sw.WriteLine("\tpublic static partial class " + xnTable.Attributes["Name"].Value);
						sw.WriteLine("\t{");

						sw.WriteLine("\t\tprivate static string TryGetEx(Dictionary<string, string> dictNew,");
						sw.WriteLine("\t\t\tstring strName, string strDefault)");
						sw.WriteLine("\t\t{");
						sw.WriteLine("\t\t\tstring strTemp;");
						sw.WriteLine();
						sw.WriteLine("\t\t\tif(dictNew.TryGetValue(strName, out strTemp))");
						sw.WriteLine("\t\t\t\treturn strTemp;");
						sw.WriteLine();
						sw.WriteLine("\t\t\treturn strDefault;");
						sw.WriteLine("\t\t}");
						sw.WriteLine();

						sw.WriteLine("\t\tpublic static void SetTranslatedStrings(Dictionary<string, string> dictNew)");
						sw.WriteLine("\t\t{");
						sw.WriteLine("\t\t\tif(dictNew == null) throw new ArgumentNullException(\"dictNew\");");
						sw.WriteLine();

#if DEBUG
						string strLastName = string.Empty;
#endif
						foreach(XmlNode xnData in xnTable.SelectNodes("Data"))
						{
							string strName = xnData.Attributes["Name"].Value;

							sw.WriteLine("\t\t\tm_str" + strName +
								" = TryGetEx(dictNew, \"" + strName +
								"\", m_str" + strName + ");");

#if DEBUG
							Debug.Assert((string.Compare(strLastName, strName, true) < 0),
								"Data names not sorted: " + strLastName + " - " + strName + ".");
							strLastName = strName;
#endif
						}

						sw.WriteLine("\t\t}");
						sw.WriteLine();

						sw.WriteLine("\t\tprivate static readonly string[] m_vKeyNames = {");
						XmlNodeList xnl = xnTable.SelectNodes("Data");
						for(int i = 0; i < xnl.Count; ++i)
						{
							XmlNode xnData = xnl.Item(i);
							sw.WriteLine("\t\t\t\"" + xnData.Attributes["Name"].Value +
								"\"" + ((i != xnl.Count - 1) ? "," : string.Empty));
						}

						sw.WriteLine("\t\t};");
						sw.WriteLine();

						sw.WriteLine("\t\tpublic static string[] GetKeyNames()");
						sw.WriteLine("\t\t{");
						sw.WriteLine("\t\t\treturn m_vKeyNames;");
						sw.WriteLine("\t\t}");

						foreach(XmlNode xnData in xnTable.SelectNodes("Data"))
						{
							string strName = xnData.Attributes["Name"].Value;
							string strValue = xnData.SelectSingleNode("Value").InnerText;
							if(strValue.Contains("\""))
							{
								// Console.WriteLine(strValue);
								strValue = strValue.Replace("\"", "\"\"");
							}

							sw.WriteLine();
							sw.WriteLine("\t\tprivate static string m_str" +
								strName + " =");
							sw.WriteLine("\t\t\t@\"" + strValue + "\";");

							sw.WriteLine("\t\t/// <summary>");
							sw.WriteLine("\t\t/// Look up a localized string similar to");
							sw.WriteLine("\t\t/// '" + StrUtil.StringToHtml(strValue) + "'.");
							sw.WriteLine("\t\t/// </summary>");
							sw.WriteLine("\t\tpublic static string " +
								strName);
							sw.WriteLine("\t\t{");
							sw.WriteLine("\t\t\tget { return m_str" + strName +
								"; }");
							sw.WriteLine("\t\t}");
						}

						sw.WriteLine("\t}"); // Close class
						sw.WriteLine("}");
					}
				}
			}
		}
	}
}
