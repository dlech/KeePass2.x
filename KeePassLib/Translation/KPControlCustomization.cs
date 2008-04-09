/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Xml.Serialization;
using System.Globalization;

namespace KeePassLib.Translation
{
	public sealed class KpccLayout
	{
		public enum LayoutParameterEx
		{
			X, Y, Width, Height
		}

		private const string m_strControlRelative = "%c";
		private static readonly CultureInfo m_lclParser = new CultureInfo("en-US", false);
		private const NumberStyles m_nsParser = NumberStyles.AllowLeadingSign |
			NumberStyles.AllowDecimalPoint;

		private string m_strPosX = string.Empty;
		[XmlAttribute]
		public string X
		{
			get { return m_strPosX; }
			set { m_strPosX = value; }
		}

		private string m_strPosY = string.Empty;
		[XmlAttribute]
		public string Y
		{
			get { return m_strPosY; }
			set { m_strPosY = value; }
		}

		private string m_strSizeW = string.Empty;
		[XmlAttribute]
		public string Width
		{
			get { return m_strSizeW; }
			set { m_strSizeW = value; }
		}

		private string m_strSizeH = string.Empty;
		[XmlAttribute]
		public string Height
		{
			get { return m_strSizeH; }
			set { m_strSizeH = value; }
		}

		public void SetControlRelativeValue(LayoutParameterEx lp, string strValue)
		{
			Debug.Assert(strValue != null);
			if(strValue == null) throw new ArgumentNullException("strValue");

			if(strValue.Length > 0) strValue += m_strControlRelative;

			if(lp == LayoutParameterEx.X) m_strPosX = strValue;
			else if(lp == LayoutParameterEx.Y) m_strPosY = strValue;
			else if(lp == LayoutParameterEx.Width) m_strSizeW = strValue;
			else if(lp == LayoutParameterEx.Height) m_strSizeH = strValue;
			else { Debug.Assert(false); }
		}

#if !KeePassLibSD
		internal void ApplyTo(Control c)
		{
			Debug.Assert(c != null); if(c == null) return;

			int? v;
			v = GetModControlParameter(c, LayoutParameterEx.X, m_strPosX);
			if(v.HasValue) c.Left = v.Value;
			v = GetModControlParameter(c, LayoutParameterEx.Y, m_strPosY);
			if(v.HasValue) c.Top = v.Value;
			v = GetModControlParameter(c, LayoutParameterEx.Width, m_strSizeW);
			if(v.HasValue) c.Width = v.Value;
			v = GetModControlParameter(c, LayoutParameterEx.Height, m_strSizeH);
			if(v.HasValue) c.Height = v.Value;
		}

		private static int? GetModControlParameter(Control c, LayoutParameterEx p,
			string strModParam)
		{
			if(strModParam.Length == 0) return null;

			Debug.Assert(c.Left == c.Location.X);
			Debug.Assert(c.Top == c.Location.Y);
			Debug.Assert(c.Width == c.Size.Width);
			Debug.Assert(c.Height == c.Size.Height);

			int iPrev;
			if(p == LayoutParameterEx.X) iPrev = c.Left;
			else if(p == LayoutParameterEx.Y) iPrev = c.Top;
			else if(p == LayoutParameterEx.Width) iPrev = c.Width;
			else if(p == LayoutParameterEx.Height) iPrev = c.Height;
			else { Debug.Assert(false); return null; }

			int iResult;
			double? dRel = ToControlRelativePercent(strModParam);
			if(dRel.HasValue)
			{
				iResult = iPrev + (int)((dRel * (double)iPrev) / 100.0);
			}
			else { Debug.Assert(false); return null; }

			return iResult;
		}

		public static double? ToControlRelativePercent(string strEncoded)
		{
			Debug.Assert(strEncoded != null);
			if(strEncoded == null) throw new ArgumentNullException("strEncoded");

			if(strEncoded.Length == 0) return null;

			if(strEncoded.EndsWith(m_strControlRelative))
			{
				double dRel;
				if(double.TryParse(strEncoded.Substring(0, strEncoded.Length -
					m_strControlRelative.Length), m_nsParser, m_lclParser, out dRel))
				{
					return dRel;
				}
				else { Debug.Assert(false); return null; }
			}
			
			Debug.Assert(false);
			return null;
		}
#endif

		public static string ToControlRelativeString(string strEncoded)
		{
			Debug.Assert(strEncoded != null);
			if(strEncoded == null) throw new ArgumentNullException("strEncoded");

			if(strEncoded.Length == 0) return string.Empty;

			if(strEncoded.EndsWith(m_strControlRelative))
				return strEncoded.Substring(0, strEncoded.Length -
					m_strControlRelative.Length);

			Debug.Assert(false);
			return string.Empty;
		}
	}

	public sealed class KPControlCustomization
	{
		private string m_strMemberName;
		/// <summary>
		/// Member variable name of the control to be translated.
		/// </summary>
		[XmlAttribute]
		public string Name
		{
			get { return m_strMemberName; }
			set { m_strMemberName = value; }
		}

		private string m_strText = string.Empty;
		public string Text
		{
			get { return m_strText; }
			set { m_strText = value; }
		}

		private string m_strEngText = string.Empty;
		[XmlIgnore]
		public string TextEnglish
		{
			get { return m_strEngText; }
			set { m_strEngText = value; }
		}

#if !KeePassLibSD
		internal void ApplyTo(Control c)
		{
			if((m_strText.Length > 0) && (c.Text.Length > 0))
				c.Text = m_strText;

			m_layout.ApplyTo(c);
		}
#endif

		private KpccLayout m_layout = new KpccLayout();
		public KpccLayout Layout
		{
			get { return m_layout; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_layout = value;
			}
		}
	}
}
