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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using KeePass.Native;
using KeePass.Resources;
using KeePass.Util;

using KeePassLib.Utility;

namespace KeePass.UI
{
	[Flags]
	public enum VtdFlags
	{
		None = 0,
		EnableHyperlinks = 0x0001,
		UseHIconMain = 0x0002,
		UseHIconFooter = 0x0004,
		AllowDialogCancellation = 0x0008,
		UseCommandLinks = 0x0010,
		UseCommandLinksNoIcon = 0x0020,
		ExpandFooterArea = 0x0040,
		ExpandedByDefault = 0x0080,
		VerificationFlagChecked = 0x0100,
		ShowProgressBar = 0x0200,
		ShowMarqueeProgressBar = 0x0400,
		CallbackTimer = 0x0800,
		PositionRelativeToWindow = 0x1000,
		RtlLayout = 0x2000,
		NoDefaultRadioButton = 0x4000
	}

	[Flags]
	internal enum VtdCommonButtonFlags
	{
		None = 0,
		OkButton = 0x0001, // Return value: IDOK = DialogResult.OK
		YesButton = 0x0002, // Return value: IDYES
		NoButton = 0x0004, // Return value: IDNO
		CancelButton = 0x0008, // Return value: IDCANCEL
		RetryButton = 0x0010, // Return value: IDRETRY
		CloseButton = 0x0020  // Return value: IDCLOSE
	}

	public enum VtdIcon
	{
		None = 0,
		Warning = 0xFFFF,
		Error = 0xFFFE,
		Information = 0xFFFD,
		Shield = 0xFFFC
	}

	public enum VtdCustomIcon
	{
		None = 0,
		Question = 1
	}

	internal enum VtdNtf
	{
		Created = 0,
		Navigated = 1,
		ButtonClicked = 2,
		HyperlinkClicked = 3,
		Timer = 4,
		Destroyed = 5,
		RadioButtonClicked = 6,
		DialogConstructed = 7,
		VerificationClicked = 8,
		Help = 9,
		ExpandoButtonClicked = 10
	}

	// Pack = 4 required for 64-bit compatibility
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
	internal struct VtdButton
	{
		public int ID;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Text;

		public VtdButton(bool _)
		{
			this.ID = (int)DialogResult.Cancel;
			this.Text = string.Empty;
		}
	}

	// Pack = 4 required for 64-bit compatibility
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
	internal struct VtdConfig
	{
		public uint cbSize;
		public IntPtr hwndParent;
		public IntPtr hInstance;
		
		[MarshalAs(UnmanagedType.U4)]
		public VtdFlags dwFlags;

		[MarshalAs(UnmanagedType.U4)]
		public VtdCommonButtonFlags dwCommonButtons;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszWindowTitle;

		public IntPtr hMainIcon;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszMainInstruction;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszContent;

		public uint cButtons;
		public IntPtr pButtons;
		public int nDefaultButton;
		public uint cRadioButtons;
		public IntPtr pRadioButtons;
		public int nDefaultRadioButton;
		
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszVerificationText;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszExpandedInformation;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszExpandedControlText;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszCollapsedControlText;

		public IntPtr hFooterIcon;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszFooter;

		public TaskDialogCallbackProc pfCallback;
		public IntPtr lpCallbackData;
		public uint cxWidth;

		public VtdConfig(bool _)
		{
			cbSize = (uint)Marshal.SizeOf(typeof(VtdConfig));
			hwndParent = IntPtr.Zero;
			hInstance = IntPtr.Zero;

			dwFlags = VtdFlags.None;
			if(Program.Translation.Properties.RightToLeft) dwFlags |= VtdFlags.RtlLayout;

			dwCommonButtons = VtdCommonButtonFlags.None;
			pszWindowTitle = null;
			hMainIcon = IntPtr.Zero;
			pszMainInstruction = string.Empty;
			pszContent = string.Empty;
			cButtons = 0;
			pButtons = IntPtr.Zero;
			nDefaultButton = 0;
			cRadioButtons = 0;
			pRadioButtons = IntPtr.Zero;
			nDefaultRadioButton = 0;
			pszVerificationText = null;
			pszExpandedInformation = null;
			pszExpandedControlText = null;
			pszCollapsedControlText = null;
			hFooterIcon = IntPtr.Zero;
			pszFooter = null;
			pfCallback = null;
			lpCallbackData = IntPtr.Zero;
			cxWidth = 0;
		}
	}

	internal delegate int TaskDialogCallbackProc(IntPtr hwnd, uint uNotification,
		UIntPtr wParam, IntPtr lParam, IntPtr lpRefData);

	public sealed class VistaTaskDialog
	{
		private const int VtdConfigSize32 = 96;
		private const int VtdConfigSize64 = 160;

		private VtdConfig m_cfg = new VtdConfig(true);
		private int m_iResult = (int)DialogResult.Cancel;
		private bool m_bVerification = false;

		private readonly List<VtdButton> m_lButtons = new List<VtdButton>();

		// private IntPtr m_hWnd = IntPtr.Zero;

		public string WindowTitle
		{
			get { return m_cfg.pszWindowTitle; }
			set { m_cfg.pszWindowTitle = value; }
		}

		public string MainInstruction
		{
			get { return m_cfg.pszMainInstruction; }
			set { m_cfg.pszMainInstruction = value; }
		}

		internal ReadOnlyCollection<VtdButton> Buttons
		{
			get { return m_lButtons.AsReadOnly(); }
		}

		public string Content
		{
			get { return m_cfg.pszContent; }
			set { m_cfg.pszContent = value; }
		}

		public bool CommandLinks
		{
			get { return ((m_cfg.dwFlags & VtdFlags.UseCommandLinks) != VtdFlags.None); }
			set
			{
				if(value) m_cfg.dwFlags |= VtdFlags.UseCommandLinks;
				else m_cfg.dwFlags &= ~VtdFlags.UseCommandLinks;
			}
		}

		public int DefaultButtonID
		{
			get { return m_cfg.nDefaultButton; }
			set { m_cfg.nDefaultButton = value; }
		}

		public bool EnableHyperlinks
		{
			get { return ((m_cfg.dwFlags & VtdFlags.EnableHyperlinks) != VtdFlags.None); }
			set
			{
				if(value) m_cfg.dwFlags |= VtdFlags.EnableHyperlinks;
				else m_cfg.dwFlags &= ~VtdFlags.EnableHyperlinks;
			}
		}

		public string ExpandedInformation
		{
			get { return m_cfg.pszExpandedInformation; }
			set { m_cfg.pszExpandedInformation = value; }
		}

		public string ExpandedControlText
		{
			get { return m_cfg.pszExpandedControlText; }
			set { m_cfg.pszExpandedControlText = value; }
		}

		public string CollapsedControlText
		{
			get { return m_cfg.pszCollapsedControlText; }
			set { m_cfg.pszCollapsedControlText = value; }
		}

		public bool ExpandedByDefault
		{
			get { return ((m_cfg.dwFlags & VtdFlags.ExpandedByDefault) != VtdFlags.None); }
			set
			{
				if(value) m_cfg.dwFlags |= VtdFlags.ExpandedByDefault;
				else m_cfg.dwFlags &= ~VtdFlags.ExpandedByDefault;
			}
		}

		public string FooterText
		{
			get { return m_cfg.pszFooter; }
			set { m_cfg.pszFooter = value; }
		}

		public string VerificationText
		{
			get { return m_cfg.pszVerificationText; }
			set { m_cfg.pszVerificationText = value; }
		}

		public int Result
		{
			get { return m_iResult; }
		}

		public bool ResultVerificationChecked
		{
			get { return m_bVerification; }
		}

		public event EventHandler<LinkClickedEventArgs> LinkClicked;

		public VistaTaskDialog()
		{
		}

		public void AddButton(int iResult, string strCommand, string strDescription)
		{
			if(strCommand == null) throw new ArgumentNullException("strCommand");

			VtdButton btn = new VtdButton(true);

			if(strDescription == null) btn.Text = strCommand;
			else btn.Text = strCommand + "\n" + strDescription;

			btn.ID = iResult;

			m_lButtons.Add(btn);
		}

		public void SetIcon(VtdIcon vtdIcon)
		{
			m_cfg.dwFlags &= ~VtdFlags.UseHIconMain;
			m_cfg.hMainIcon = new IntPtr((int)vtdIcon);
		}

		public void SetIcon(VtdCustomIcon vtdIcon)
		{
			if(vtdIcon == VtdCustomIcon.Question)
				SetIcon(SystemIcons.Question.Handle);
		}

		public void SetIcon(IntPtr hIcon)
		{
			m_cfg.dwFlags |= VtdFlags.UseHIconMain;
			m_cfg.hMainIcon = hIcon;
		}

		public void SetFooterIcon(VtdIcon vtdIcon)
		{
			m_cfg.dwFlags &= ~VtdFlags.UseHIconFooter;
			m_cfg.hFooterIcon = new IntPtr((int)vtdIcon);
		}

		private void ButtonsToPtr()
		{
			if(m_lButtons.Count == 0) { m_cfg.pButtons = IntPtr.Zero; return; }

			int nConfigSize = Marshal.SizeOf(typeof(VtdButton));
			m_cfg.pButtons = Marshal.AllocHGlobal(m_lButtons.Count * nConfigSize);
			m_cfg.cButtons = (uint)m_lButtons.Count;

			for(int i = 0; i < m_lButtons.Count; ++i)
			{
				long l = m_cfg.pButtons.ToInt64() + (i * nConfigSize);
				Marshal.StructureToPtr(m_lButtons[i], new IntPtr(l), false);
			}
		}

		private void FreeButtonsPtr()
		{
			if(m_cfg.pButtons == IntPtr.Zero) return;

			int nConfigSize = Marshal.SizeOf(typeof(VtdButton));
			for(int i = 0; i < m_lButtons.Count; ++i)
			{
				long l = m_cfg.pButtons.ToInt64() + (i * nConfigSize);
				Marshal.DestroyStructure(new IntPtr(l), typeof(VtdButton));
			}

			Marshal.FreeHGlobal(m_cfg.pButtons);
			m_cfg.pButtons = IntPtr.Zero;
			m_cfg.cButtons = 0;
		}

		public bool ShowDialog()
		{
			return ShowDialog(null);
		}

		public bool ShowDialog(Form fParent)
		{
			MessageService.ExternalIncrementMessageCount();

			Form f = fParent;
			if(f == null) f = MessageService.GetTopForm();
			if(f == null) f = GlobalWindowManager.TopWindow;
			if(f == null) f = Program.MainForm;

#if DEBUG
			if(GlobalWindowManager.TopWindow != null)
			{
				Debug.Assert(f == GlobalWindowManager.TopWindow);
			}
			if(Program.MainForm != null) // Skip check for TrlUtil
			{
				Debug.Assert((f == MessageService.GetTopForm()) || (f == Program.MainForm));
			}
#endif

			bool bResult;
			if((f == null) || !f.InvokeRequired)
				bResult = InternalShowDialog(f);
			else
				bResult = (bool)f.Invoke(new InternalShowDialogDelegate(
					this.InternalShowDialog), f);

			MessageService.ExternalDecrementMessageCount();
			return bResult;
		}

		private delegate bool InternalShowDialogDelegate(Form fParent);

		private bool InternalShowDialog(Form fParent)
		{
			if(IntPtr.Size == 4)
				{ Debug.Assert(Marshal.SizeOf(typeof(VtdConfig)) == VtdConfigSize32); }
			else if(IntPtr.Size == 8)
				{ Debug.Assert(Marshal.SizeOf(typeof(VtdConfig)) == VtdConfigSize64); }
			else { Debug.Assert(false); }

			m_cfg.cbSize = (uint)Marshal.SizeOf(typeof(VtdConfig));

			if(fParent == null) m_cfg.hwndParent = IntPtr.Zero;
			else
			{
				try { m_cfg.hwndParent = fParent.Handle; }
				catch(Exception)
				{
					Debug.Assert(false);
					m_cfg.hwndParent = IntPtr.Zero;
				}
			}

			int pnButton = 0, pnRadioButton = 0;
			bool bVerification = false;

			try { ButtonsToPtr(); }
			catch(Exception) { Debug.Assert(false); return false; }

			m_cfg.pfCallback = this.OnTaskDialogCallback;

			try
			{
				using(EnableThemingInScope etis = new EnableThemingInScope(true))
				{
					if(NativeMethods.TaskDialogIndirect(ref m_cfg, out pnButton,
						out pnRadioButton, out bVerification) != 0)
						throw new NotSupportedException();
				}
			}
			catch(Exception) { return false; }
			finally
			{
				try
				{
					m_cfg.pfCallback = null;
					FreeButtonsPtr();
				}
				catch(Exception) { Debug.Assert(false); }
			}

			m_iResult = pnButton;
			m_bVerification = bVerification;
			return true;
		}

		private int OnTaskDialogCallback(IntPtr hwnd, uint uNotification,
			UIntPtr wParam, IntPtr lParam, IntPtr lpRefData)
		{
			try
			{
				// if((uNotification == (uint)VtdNtf.Created) ||
				//	(uNotification == (uint)VtdNtf.DialogConstructed))
				//	UpdateHWnd(hwnd);
				// else if(uNotification == (uint)VtdNtf.Destroyed)
				//	UpdateHWnd(IntPtr.Zero);

				if((uNotification == (uint)VtdNtf.HyperlinkClicked) && this.EnableHyperlinks &&
					(lParam != IntPtr.Zero))
				{
					string str = Marshal.PtrToStringUni(lParam);
					if(str != null)
					{
						if(str.StartsWith("http:", StrUtil.CaseIgnoreCmp) ||
							str.StartsWith("https:", StrUtil.CaseIgnoreCmp))
							WinUtil.OpenUrl(str, null);
						else if(this.LinkClicked != null)
						{
							LinkClickedEventArgs e = new LinkClickedEventArgs(str);
							this.LinkClicked(this, e);
						}
						else { Debug.Assert(false); }
					}
					else { Debug.Assert(false); }
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return 0;
		}

		/* private void UpdateHWnd(IntPtr hWnd)
		{
			if(hWnd != m_hWnd) { } // Unregister m_hWnd
			// Register hWnd
			m_hWnd = hWnd;
		} */

		public static string CreateLink(string strHRef, string strText)
		{
			string strH = (strHRef ?? string.Empty);
			string strT = (strText ?? string.Empty);
			return ("<A HREF=\"" + strH + "\">" + strT + "</A>");
		}

		internal static string Unlink(string strText)
		{
			if(string.IsNullOrEmpty(strText)) return string.Empty;

			string str = strText;
			while(true)
			{
				int iS = str.IndexOf("<A HREF=\"", StrUtil.CaseIgnoreCmp);
				if(iS < 0) break;

				const string strE = "\">";
				int iE = str.IndexOf(strE, iS, StrUtil.CaseIgnoreCmp);
				if(iE < 0) { Debug.Assert(false); break; }

				const string strC = "</A>";
				int iC = str.IndexOf(strC, iE, StrUtil.CaseIgnoreCmp);
				if(iC < 0) { Debug.Assert(false); break; }

				str = str.Remove(iC, strC.Length);
				str = str.Remove(iS, iE - iS + strE.Length);
			}

			return str;
		}

		public static bool ShowMessageBox(string strContent, string strMainInstruction,
			string strWindowTitle, VtdIcon vtdIcon, Form fParent)
		{
			return (ShowMessageBoxEx(strContent, strMainInstruction, strWindowTitle,
				vtdIcon, fParent, null, 0, null, 0) >= 0);
		}

		public static int ShowMessageBoxEx(string strContent, string strMainInstruction,
			string strWindowTitle, VtdIcon vtdIcon, Form fParent,
			string strButton1, int iResult1, string strButton2, int iResult2)
		{
			VistaTaskDialog vtd = new VistaTaskDialog();

			vtd.CommandLinks = false;

			if(strContent != null) vtd.Content = strContent;
			if(strMainInstruction != null) vtd.MainInstruction = strMainInstruction;
			if(strWindowTitle != null) vtd.WindowTitle = strWindowTitle;

			vtd.SetIcon(vtdIcon);

			bool bCustomButton = false;
			if(!string.IsNullOrEmpty(strButton1))
			{
				vtd.AddButton(iResult1, strButton1, null);
				bCustomButton = true;
			}
			if(!string.IsNullOrEmpty(strButton2))
			{
				vtd.AddButton(iResult2, strButton2, null);
				bCustomButton = true;
			}

			if(!vtd.ShowDialog(fParent)) return -1;
			return (bCustomButton ? vtd.Result : 0);
		}
	}
}
