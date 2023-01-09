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
using System.Text;
using System.Diagnostics;

using KeePassLib;
using KeePassLib.Interfaces;

namespace KeePass.Util.Spr
{
	[Flags]
	public enum SprCompileFlags
	{
		None = 0,

		AppPaths = 0x1, // Paths to IE, Firefox, Opera, ...
		PickChars = 0x2, // {PICKCHARS}, {PICKFIELD}
		EntryStrings = 0x4,
		EntryStringsSpecial = 0x8, // {URL:RMVSCM}, ...
		EntryProperties = 0x200000, // {UUID}, ...
		PasswordEnc = 0x10,
		Group = 0x20,
		Paths = 0x40, // App-dir, doc-dir, path sep, ...
		AutoType = 0x80, // Replacements like {CLEARFIELD}, ...
		DateTime = 0x100,
		References = 0x200,
		EnvVars = 0x400,
		NewPassword = 0x800,
		HmacOtp = 0x1000, // {HMACOTP}, {TIMEOTP}, ...
		Comments = 0x2000,
		TextTransforms = 0x10000,
		Env = 0x20000, // {BASE}, ...
		Run = 0x40000, // Running other (console) applications
		DataActive = 0x80000, // {CLIPBOARD-SET:/.../}, ...
		DataNonActive = 0x100000, // {CLIPBOARD}, ...

		ExtActive = 0x4000, // Active transformations provided by plugins
		ExtNonActive = 0x8000, // Non-active transformations provided by plugins

		// Next free: 0x400000
		All = 0x3FFFFF,

		// Internal:
		UIInteractive = (SprCompileFlags.PickChars | SprCompileFlags.Run),
		StateChanging = (SprCompileFlags.NewPassword | SprCompileFlags.HmacOtp),

		Active = (SprCompileFlags.UIInteractive | SprCompileFlags.StateChanging |
			SprCompileFlags.DataActive | SprCompileFlags.ExtActive),
		NonActive = (SprCompileFlags.All & ~SprCompileFlags.Active),

		Deref = (SprCompileFlags.EntryStrings | SprCompileFlags.EntryStringsSpecial |
			SprCompileFlags.References)
	}

	public sealed class SprContext
	{
		private PwEntry m_pe = null;
		public PwEntry Entry
		{
			get { return m_pe; }
			set { m_pe = value; }
		}

		private PwDatabase m_pd = null;
		public PwDatabase Database
		{
			get { return m_pd; }
			set { m_pd = value; }
		}

		private string m_strBase = null;
		/// <summary>
		/// The parent string, like e.g. the input string before any
		/// override has been applied.
		/// </summary>
		public string Base
		{
			get { return m_strBase; }
			set { m_strBase = value; }
		}

		private bool m_bBaseIsEnc = false;
		/// <summary>
		/// Specifies whether <c>Base</c> has been content-transformed already.
		/// </summary>
		public bool BaseIsEncoded
		{
			get { return m_bBaseIsEnc; }
			set { m_bBaseIsEnc = value; }
		}

		private bool m_bMakeAT = false;
		public bool EncodeAsAutoTypeSequence
		{
			get { return m_bMakeAT; }
			set { m_bMakeAT = value; }
		}

		private bool m_bMakeCmd = false;
		public bool EncodeForCommandLine
		{
			get { return m_bMakeCmd; }
			set { m_bMakeCmd = value; }
		}

		private bool m_bForcePlainTextPasswords = true;
		public bool ForcePlainTextPasswords
		{
			get { return m_bForcePlainTextPasswords; }
			set { m_bForcePlainTextPasswords = value; }
		}

		private SprCompileFlags m_flags = SprCompileFlags.All;
		public SprCompileFlags Flags
		{
			get { return m_flags; }
			set { m_flags = value; }
		}

		private SprRefCache m_refCache = new SprRefCache();
		/// <summary>
		/// Used internally by <c>SprEngine</c>; don't modify it.
		/// </summary>
		internal SprRefCache RefCache
		{
			get { return m_refCache; }
		}

		public SprContext() { }

		public SprContext(PwEntry pe, PwDatabase pd, SprCompileFlags fl)
		{
			Init(pe, pd, false, false, fl);
		}

		public SprContext(PwEntry pe, PwDatabase pd, SprCompileFlags fl,
			bool bEncodeAsAutoTypeSequence, bool bEncodeForCommandLine)
		{
			Init(pe, pd, bEncodeAsAutoTypeSequence, bEncodeForCommandLine, fl);
		}

		private void Init(PwEntry pe, PwDatabase pd, bool bAT, bool bCmd,
			SprCompileFlags fl)
		{
			m_pe = pe;
			m_pd = pd;
			m_bMakeAT = bAT;
			m_bMakeCmd = bCmd;
			m_flags = fl;
		}

		public SprContext Clone()
		{
			return (SprContext)this.MemberwiseClone();
		}

		/// <summary>
		/// Used by <c>SprEngine</c> internally; do not use.
		/// </summary>
		internal SprContext WithoutContentTransformations()
		{
			SprContext ctx = Clone();

			ctx.m_bMakeAT = false;
			ctx.m_bMakeCmd = false;
			// ctx.m_bNoUrlSchemeOnce = false;

			Debug.Assert(object.ReferenceEquals(m_pe, ctx.m_pe));
			Debug.Assert(object.ReferenceEquals(m_pd, ctx.m_pd));
			Debug.Assert(object.ReferenceEquals(m_refCache, ctx.m_refCache));
			return ctx;
		}
	}

	public sealed class SprEventArgs : EventArgs
	{
		private string m_str = string.Empty;
		public string Text
		{
			get { return m_str; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_str = value;
			}
		}

		private SprContext m_ctx = null;
		public SprContext Context
		{
			get { return m_ctx; }
		}

		public SprEventArgs() { }

		public SprEventArgs(string strText, SprContext ctx)
		{
			if(strText == null) throw new ArgumentNullException("strText");
			// ctx == null is allowed

			m_str = strText;
			m_ctx = ctx;
		}
	}
}
