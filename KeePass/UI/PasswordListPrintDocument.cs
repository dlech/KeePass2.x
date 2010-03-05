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

/*
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Diagnostics;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Security;

namespace KeePass.UI
{
	internal sealed class PasswordListLine
	{
		public PwIcon IconID = PwIcon.None;
		public PwUuid CustomIcon = null;
		public List<string> TextFields = new List<string>();
	}

	public sealed class PasswordListPrintDocument : PrintDocument
	{
		private PwGroup m_pgDataSource = null;

		private Font m_fontNormal = null;
		private Font m_fontPassword = null;

		private List<PasswordListLine> m_vLines = new List<PasswordListLine>();
		private int m_nCurrentLine = 0;

		public PasswordListPrintDocument(PwGroup pgDataSource)
		{
			Debug.Assert(pgDataSource != null);
			if(pgDataSource == null) throw new ArgumentNullException("pgDataSource");

			m_pgDataSource = pgDataSource;
		}

		private void DisposePrinterFont(ref Font f)
		{
			if(f != null)
			{
				f.Dispose();
				f = null;
			}
		}

		protected override void OnBeginPrint(PrintEventArgs e)
		{
			base.OnBeginPrint(e);

			RenderDocument(e);

			m_fontNormal = new Font(FontFamily.GenericSansSerif, 10.0f);
			m_fontPassword = new Font(FontFamily.GenericMonospace, 10.0f);
		}

		protected override void OnEndPrint(PrintEventArgs e)
		{
			base.OnEndPrint(e);

			DisposePrinterFont(ref m_fontNormal);
			DisposePrinterFont(ref m_fontPassword);
		}

		protected override void OnPrintPage(PrintPageEventArgs e)
		{
			base.OnPrintPage(e);

			Graphics g = e.Graphics;

			float fLeft = e.MarginBounds.Left;
			float fRight = e.MarginBounds.Right;
			float fTop = e.MarginBounds.Top;
			float fBottom = e.MarginBounds.Bottom;
			float fWidth = fRight - fLeft;

			string strTestText = @"WJygpqQIL|°";
			float fNormalHeight = TextRenderer.MeasureText(g, strTestText,
				m_fontNormal).Height;
			float fPasswordHeight = TextRenderer.MeasureText(g, strTestText,
				m_fontPassword).Height;

			float fLineHeight = Math.Max(fNormalHeight, fPasswordHeight);

			float fCurrentY = fTop;

			while(true)
			{
				if((fCurrentY + fLineHeight) >= fBottom)
					break;

				PasswordListLine pll = m_vLines[m_nCurrentLine];
				int nTextCount = pll.TextFields.Count;

				for(int iField = 0; iField < nTextCount; ++iField)
				{
					PointF ptText = new PointF((fWidth * iField) / nTextCount,
						fCurrentY);

					g.DrawString(pll.TextFields[iField], m_fontNormal,
						Brushes.Black, ptText);
				}

				fCurrentY += fLineHeight;

				++m_nCurrentLine;
				if(m_nCurrentLine == m_vLines.Count) break;
			}

			e.HasMorePages = (m_nCurrentLine != m_vLines.Count);
		}

		private void RenderDocument(PrintEventArgs e)
		{
			m_vLines.Clear();
			m_nCurrentLine = 0;

			GroupHandler gh = delegate(PwGroup pg)
			{
				return true;
			};

			EntryHandler eh = delegate(PwEntry pe)
			{
				PasswordListLine pll = new PasswordListLine();

				pll.IconID = pe.IconID;
				pll.CustomIcon = pe.CustomIconUuid;
				pll.TextFields.Add(pe.Strings.ReadSafe(PwDefs.TitleField));
				pll.TextFields.Add(pe.Strings.ReadSafe(PwDefs.UserNameField));
				pll.TextFields.Add(pe.Strings.ReadSafe(PwDefs.PasswordField));
				pll.TextFields.Add(pe.Strings.ReadSafe(PwDefs.UrlField));
				pll.TextFields.Add(pe.Strings.ReadSafe(PwDefs.NotesField));

				m_vLines.Add(pll);
				return true;
			};

			m_pgDataSource.TraverseTree(TraversalMethod.PreOrder, gh, eh);
		}
	}
}
*/
