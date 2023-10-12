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

using KeePassLib.Utility;

namespace KeePass.Util.Spr
{
	internal sealed class SprRefCache
	{
		private sealed class SprRefCacheItem
		{
			public readonly string Ref;
			public readonly string Value;
			public readonly uint Context;

			public SprRefCacheItem(string strRef, string strValue, uint uContext)
			{
				this.Ref = strRef;
				this.Value = strValue;
				this.Context = uContext;
			}
		}

		private readonly List<SprRefCacheItem> m_l = new List<SprRefCacheItem>();

		public SprRefCache()
		{
		}

		/// <summary>
		/// Hash all settings of <paramref name="ctx" /> that may
		/// affect the encoded value of a field reference.
		/// </summary>
		private static uint HashContext(SprContext ctx)
		{
			if(ctx == null) { Debug.Assert(false); return 0; }

			uint u = 0;

			if(ctx.ForcePlainTextPasswords) u |= 1;
			if(ctx.EncodeForCommandLine) u |= 2;
			if(ctx.EncodeAsAutoTypeSequence) u |= 4;

			return u;
		}

		public void Clear()
		{
			m_l.Clear();
		}

		private string Get(string strRef, uint uCtx)
		{
			if(strRef == null) { Debug.Assert(false); return null; }

			foreach(SprRefCacheItem ci in m_l)
			{
				if(ci.Context != uCtx) continue;

				if(string.Equals(strRef, ci.Ref, StrUtil.CaseIgnoreCmp))
					return ci.Value;
			}

			return null;
		}

		public bool Add(string strRef, string strValue, SprContext ctx)
		{
			if(strRef == null) throw new ArgumentNullException("strRef");
			if(strValue == null) throw new ArgumentNullException("strValue");

			uint uCtx = HashContext(ctx);

			if(Get(strRef, uCtx) != null)
			{
				Debug.Assert(false);
				return false; // Exists already, do not overwrite
			}

			m_l.Add(new SprRefCacheItem(strRef, strValue, uCtx));
			return true;
		}

		public string Fill(string strText, SprContext ctx)
		{
			if(strText == null) { Debug.Assert(false); return string.Empty; }

			string str = strText;
			uint uCtx = HashContext(ctx);

			foreach(SprRefCacheItem ci in m_l)
			{
				if(ci.Context != uCtx) continue;

				// str = str.Replace(ci.Ref, ci.Value);
				str = StrUtil.ReplaceCaseInsensitive(str, ci.Ref, ci.Value);
			}

			return str;
		}
	}
}
