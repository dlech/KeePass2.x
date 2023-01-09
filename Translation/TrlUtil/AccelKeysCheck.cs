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
using System.Windows.Forms;

using KeePassLib.Delegates;
using KeePassLib.Translation;
using KeePassLib.Utility;

namespace TrlUtil
{
	public static class AccelKeysCheck
	{
		public static void Validate(KPTranslation trl, List<string> lErrors)
		{
			if(trl == null) { Debug.Assert(false); return; }

			foreach(KPFormCustomization kpfc in trl.Forms)
			{
				if(kpfc == null) { Debug.Assert(false); continue; }

				Validate(kpfc, kpfc.FormEnglish, new Dictionary<char, string>(), lErrors);
			}
		}

		private static void Validate(KPFormCustomization kpfc, Control cParent,
			Dictionary<char, string> d, List<string> lErrors)
		{
			if(kpfc == null) { Debug.Assert(false); return; }
			if(cParent == null) { Debug.Assert(false); return; }

			List<Control> lToRecInto = new List<Control>();

			foreach(Control c in cParent.Controls)
			{
				if(c == null) { Debug.Assert(false); continue; }
				if(c == cParent) { Debug.Assert(false); continue; }

				Debug.Assert((cParent is TabControl) == (c is TabPage));

				if(c is TabPage) lToRecInto.Add(c);
				else Validate(kpfc, c, d, lErrors); // Same context as parent

				string strText = Translate(kpfc, c);
				if(string.IsNullOrEmpty(strText)) continue;

				string strName = (string.IsNullOrEmpty(c.Name) ? "<Unknown>" : c.Name);
				string strID = "'" + kpfc.FullName + "." + strName + "'," +
					MessageService.NewLine + "text: \"" + strText + "\".";

				string strError;
				char chKey = GetAccelKey(strText, out strError);
				if(strError != null)
					lErrors.Add("Control: " + strID + MessageService.NewParagraph +
						strError);
				if(chKey == char.MinValue) continue;

				string strExist;
				if(d.TryGetValue(chKey, out strExist))
					lErrors.Add("Accelerator key '" + chKey.ToString() +
						"' collision:" + MessageService.NewParagraph +
						"Control 1: " + strExist + MessageService.NewParagraph +
						"Control 2: " + strID);
				else d.Add(chKey, strID);
			}

			foreach(Control cSub in lToRecInto)
			{
				Dictionary<char, string> dSub = new Dictionary<char, string>(d);
				Validate(kpfc, cSub, dSub, lErrors); // New context
			}
		}

		private static string Translate(KPFormCustomization kpfc, Control c)
		{
			string strName = c.Name;
			if(string.IsNullOrEmpty(strName)) return string.Empty;

			foreach(KPControlCustomization cc in kpfc.Controls)
			{
				if(cc.Name == strName)
				{
					if(!string.IsNullOrEmpty(cc.TextEnglish))
					{
						Debug.Assert(c.Text == cc.TextEnglish);
					}

					return (!string.IsNullOrEmpty(cc.Text) ? cc.Text : c.Text);
				}
			}

			return c.Text;
		}

		private static char GetAccelKey(string strText, out string strError)
		{
			strError = null;
			if(string.IsNullOrEmpty(strText)) return char.MinValue;

			strText = strText.Replace(@"&&", string.Empty);

			int iL = strText.IndexOf('&');
			if(iL < 0) return char.MinValue;

			int iR = strText.LastIndexOf('&');
			if(iR != iL)
			{
				strError = "The text must not contain multiple accelerator key definitions!";
				return char.MinValue;
			}

			if(iL == (strText.Length - 1))
			{
				strError = "Invalid accelerator key definition at the end of the text!";
				return char.MinValue;
			}

			return char.ToUpper(strText[iL + 1]);
		}
	}
}
