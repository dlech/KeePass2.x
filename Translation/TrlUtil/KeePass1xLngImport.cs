/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Diagnostics;

using KeePassLib.Translation;

namespace TrlUtil
{
	public static class KeePass1xLngImport
	{
		public static void Import(KPTranslation kpInto, string strFile)
		{
			if((strFile == null) || (strFile.Length == 0)) { Debug.Assert(false); return; }

			string strData = File.ReadAllText(strFile, Encoding.UTF8);

			Dictionary<string, string> dict = new Dictionary<string, string>();

			const int nStatePreEn = 0;
			const int nStateInEn = 1;
			const int nStateBetween = 2;
			const int nStateInTrl = 3;

			StringBuilder sbEn = new StringBuilder();
			StringBuilder sbTrl = new StringBuilder();
			int nState = nStatePreEn;

			for(int i = 0; i < strData.Length; ++i)
			{
				char ch = strData[i];

				if(ch == '|')
				{
					if(nState == nStatePreEn) nState = nStateInEn;
					else if(nState == nStateInEn) nState = nStateBetween;
					else if(nState == nStateBetween) nState = nStateInTrl;
					else if(nState == nStateInTrl)
					{
						dict[sbEn.ToString()] = sbTrl.ToString();

						sbEn = new StringBuilder();
						sbTrl = new StringBuilder();

						nState = nStatePreEn;
					}
				}
				else if(nState == nStateInEn) sbEn.Append(ch);
				else if(nState == nStateInTrl) sbTrl.Append(ch);
			}

			Debug.Assert(nState == nStatePreEn);

			dict[string.Empty] = string.Empty;

			MergeDict(kpInto, dict);
		}

		private static void MergeDict(KPTranslation kpInto, Dictionary<string, string> dict)
		{
			if(kpInto == null) { Debug.Assert(false); return; }
			if(dict == null) { Debug.Assert(false); return; }

			foreach(KPStringTable kpst in kpInto.StringTables)
			{
				foreach(KPStringTableItem kpsti in kpst.Strings)
				{
					if(kpsti.Value.Length == 0)
					{
						string strTrl;
						if(dict.TryGetValue(kpsti.ValueEnglish, out strTrl))
							kpsti.Value = strTrl;
					}
				}
			}

			foreach(KPFormCustomization kpfc in kpInto.Forms)
			{
				if(kpfc.Window.Text.Length == 0)
				{
					string strTrlWnd;
					if(dict.TryGetValue(kpfc.Window.TextEnglish, out strTrlWnd))
						kpfc.Window.Text = strTrlWnd;
				}

				foreach(KPControlCustomization kpcc in kpfc.Controls)
				{
					if(kpcc.Text.Length == 0)
					{
						string strTrlCtrl;
						if(dict.TryGetValue(kpcc.TextEnglish, out strTrlCtrl))
							kpcc.Text = strTrlCtrl;
					}
				}
			}
		}
	}
}
