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
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;
using KeePass.Util.XmlSerialization;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.App.Configuration
{
	internal sealed class AecItem
	{
		public readonly object Container;
		public readonly string PropertyName;
		public readonly bool Enforce;

		public AecItem(object oContainer, string strPropertyName, bool bEnforce)
		{
			this.Container = oContainer;
			this.PropertyName = strPropertyName;
			this.Enforce = bEnforce;
		}

#if DEBUG
		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", this.Container, this.PropertyName,
				this.Enforce);
		}
#endif
	}

	internal static class AppEnforcedConfig
	{
		public static bool Modify(List<AecItem> lItems, AppConfigEx cfgValues,
			bool bAllowElevate)
		{
			try
			{
				if(lItems == null) throw new ArgumentNullException("lItems");
				if(cfgValues == null) throw new ArgumentNullException("cfgValues");

				XmlDocument xdEnforced = (AppConfigSerializer.LoadEnforced(false) ??
					AppConfigEx.CreateEmptyXmlDocument());

				XmlDocument xdValues = XmlUtilEx.CreateXmlDocument();
				using(MemoryStream msW = new MemoryStream())
				{
					AppConfigSerializer.SaveEx(cfgValues, msW);

					using(MemoryStream msR = new MemoryStream(msW.ToArray(), false))
					{
						using(StreamReader sr = new StreamReader(msR, StrUtil.Utf8, true))
						{
							xdValues.LoadXml(sr.ReadToEnd());
						}
					}
				}

				foreach(AecItem it in lItems)
					Modify(xdEnforced, it, xdValues, cfgValues);

				using(MemoryStream msW = new MemoryStream())
				{
					using(XmlWriter xw = XmlUtilEx.CreateXmlWriter(msW))
					{
						xdEnforced.Save(xw);
					}

					if(Save(msW.ToArray(), bAllowElevate))
					{
						// Apply the modifications to the enforced configuration
						// of the current process, too; the enforced configuration
						// file on disk may contain additional settings that are
						// not loaded/enforced yet

						XmlDocument xdProcess = AppConfigSerializer.EnforcedConfigXml;
						if(xdProcess == null)
						{
							xdProcess = AppConfigEx.CreateEmptyXmlDocument();
							AppConfigSerializer.EnforcedConfigXml = xdProcess;
						}

						foreach(AecItem it in lItems)
							Modify(xdProcess, it, xdValues, cfgValues);

						return true;
					}
				}
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }

			return false;
		}

		private static void Modify(XmlDocument xdEnforced, AecItem it,
			XmlDocument xdValues, AppConfigEx cfgValues)
		{
			object oC = it.Container;
			PropertyInfo pi = oC.GetType().GetProperty(it.PropertyName);
			if((pi == null) || !pi.CanRead) throw new MemberAccessException();

			string strPath = XmlUtil.GetObjectXmlPath(cfgValues, oC);
			if(string.IsNullOrEmpty(strPath)) throw new KeyNotFoundException();
			strPath += "/" + XmlSerializerEx.GetXmlName(pi);

			const string strRoot = "/" + AppConfigEx.StrXmlTypeName + "/";
			if(!strPath.StartsWith(strRoot)) throw new InvalidOperationException();
			string strPathRel = strPath.Substring(strRoot.Length);

			XmlElement xe = XmlUtil.FindOrCreateElement(xdEnforced.DocumentElement,
				strPathRel, xdEnforced);
			if(xe == null) throw new InvalidOperationException();

			if(!it.Enforce)
			{
				xe.RemoveAll(); // Removes attributes and children

				while(xe != xdEnforced.DocumentElement)
				{
					XmlElement xeP = (xe.ParentNode as XmlElement);
					if(xeP == null) { Debug.Assert(false); break; }

					xeP.RemoveChild(xe);

					if(xeP.HasChildNodes || xeP.HasAttributes) break;
					xe = xeP;
				}

				return;
			}

			string strV;
			XmlNode xnV = xdValues.SelectSingleNode(strPath);

			if(xnV != null)
				strV = XmlUtil.SafeInnerXml(xnV);
			else
			{
				Type tV = pi.PropertyType;
				object oV = pi.GetValue(oC, null);
				Debug.Assert((oV == null) || (oV.GetType() == tV));

				// Cf. OptionsEnfForm.AddOption
				if(tV == typeof(bool))
					strV = XmlConvert.ToString((bool)oV);
				else if(tV == typeof(int))
					strV = XmlConvert.ToString((int)oV);
				else if(tV == typeof(uint))
					strV = XmlConvert.ToString((uint)oV);
				else if(tV == typeof(string))
				{
					strV = (oV as string);
					if(strV != string.Empty) throw new NotImplementedException();
				}
				else throw new NotImplementedException();
			}

			Debug.Assert(strV != null);
			if(string.IsNullOrEmpty(strV)) xe.IsEmpty = true;
			else xe.InnerXml = strV;

			// Fix merge attributes to ensure enforcement
			XmContext ctx = new XmContext(xdEnforced, AppConfigEx.GetNodeOptions,
				AppConfigEx.GetNodeKey);
			while(true)
			{
				string strXE = XmlUtil.GetPath(xe);
				XmNodeOptions xno = XmlUtil.GetNodeOptions(xe, strXE, ctx);

				if(xno.NodeMode != XmNodeMode.OpenOrCreate)
				{
					if(xe.HasAttribute(XmNodeOptions.AttribNodeMode))
						xe.RemoveAttribute(XmNodeOptions.AttribNodeMode);
					else { Debug.Assert(false); } // OpenOrCreate is not the default
				}

				if(xe == xdEnforced.DocumentElement) break;
				xe = (xe.ParentNode as XmlElement);
				if(xe == null) { Debug.Assert(false); break; }
			}
		}

		private static bool Save(byte[] pbXml, bool bAllowElevate)
		{
			string strFile = AppConfigSerializer.EnforcedConfigFile;
			if(string.IsNullOrEmpty(strFile)) throw new InvalidOperationException();

			try
			{
				File.WriteAllBytes(strFile, pbXml);
				return true;
			}
			catch(UnauthorizedAccessException) { }
			catch(Exception) { Debug.Assert(false); }

			if(!bAllowElevate) return false;

			string strTempFile = Program.TempFilesPool.GetTempFileName(true);
			File.WriteAllBytes(strTempFile, pbXml);

			string strHash = Convert.ToBase64String(CryptoUtil.HashSha256(pbXml));
			string strArgs = "-" + AppDefs.CommandLineOptions.ConfigEnfSetupFile +
				":\"" + NativeLib.EncodeDataToArgs(strTempFile) + "\" -" +
				AppDefs.CommandLineOptions.ConfigEnfSetupHash +
				":\"" + NativeLib.EncodeDataToArgs(strHash) + "\"";

			bool b = WinUtil.RunElevated(WinUtil.GetExecutable(), strArgs, true,
				(NativeLib.IsUnix() ? 5000 : -1));

			Program.TempFilesPool.Delete(strTempFile);

			if(!b || !File.Exists(strFile)) return false;
			byte[] pbStoredXml = File.ReadAllBytes(strFile);
			return MemUtil.ArraysEqual(pbStoredXml, pbXml);
		}

		internal static bool SetupAsChild()
		{
			string strTempFile = Program.CommandLineArgs[AppDefs.CommandLineOptions.ConfigEnfSetupFile];
			if(string.IsNullOrEmpty(strTempFile)) return false;

			string strContext = string.Empty;
			try
			{
				string strFile = AppConfigSerializer.EnforcedConfigFile;
				if(string.IsNullOrEmpty(strFile)) throw new InvalidOperationException();

				strContext = strTempFile;
				byte[] pbXml = File.ReadAllBytes(strTempFile);

				string strHashCmp = Convert.ToBase64String(CryptoUtil.HashSha256(pbXml));
				string strHashExp = Program.CommandLineArgs[AppDefs.CommandLineOptions.ConfigEnfSetupHash];
				if(strHashCmp != strHashExp) throw new InvalidDataException();

				strContext = strFile;
				File.WriteAllBytes(strFile, pbXml);
			}
			catch(Exception ex) { MessageService.ShowWarning(strContext, ex); }

			return true; // Handled command line option
		}

		internal static bool ConfirmSavingItems(Form fParent)
		{
			if(!Program.Config.Meta.PreferUserConfiguration) return true;

			string str = KPRes.ConfigEnfSaveItemsAbout + MessageService.NewParagraph +
				KPRes.ConfigEnfAdminUac + MessageService.NewParagraph +
				KPRes.ConfigEnfAllUsers + " " + KPRes.ItemsNoSensitiveEsp;

			int r = VistaTaskDialog.ShowMessageBoxEx(str, null, PwDefs.ShortProductName,
				VtdIcon.Shield, fParent, KPRes.Ok, (int)DialogResult.OK,
				KPRes.Cancel, (int)DialogResult.Cancel);
			if(r < 0)
				r = (int)MessageService.Ask(str, null, MessageBoxButtons.OKCancel);

			return (r == (int)DialogResult.OK);
		}
	}
}
