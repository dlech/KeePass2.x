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

#include "StdAfx.h"
#include <windows.h>
#include <tchar.h>
#include <string>

#include "SamplePluginCpp.h"

using namespace KeePassLib;
using namespace KeePassLib::Security;

namespace SamplePluginCpp {

bool SamplePluginCppExt::Initialize(IPluginHost^ host)
{
	m_host = host;

	ToolStripItemCollection^ tsMenu = m_host->MainWindow->ToolsMenu->DropDownItems;

	m_tsmiPopup = gcnew ToolStripMenuItem();
	m_tsmiPopup->Text = _T("Sample Plugin for Developers (C++)");
	tsMenu->Add(m_tsmiPopup);

	m_tsmiAddEntries = gcnew ToolStripMenuItem();
	m_tsmiAddEntries->Text = _T("Add Sample Entries");
	m_tsmiAddEntries->Click += gcnew EventHandler(this,
		&SamplePluginCppExt::TestAddEntries);
	m_tsmiPopup->DropDownItems->Add(m_tsmiAddEntries);

	m_tsmiSep = gcnew ToolStripSeparator();
	m_tsmiPopup->DropDownItems->Add(m_tsmiSep);

	m_tsmiTestNative = gcnew ToolStripMenuItem();
	m_tsmiTestNative->Text = _T("Test Native C++");
	m_tsmiTestNative->Click += gcnew EventHandler(this,
		&SamplePluginCppExt::TestNative);
	m_tsmiPopup->DropDownItems->Add(m_tsmiTestNative);

	return true;
}

void SamplePluginCppExt::Terminate()
{
	m_tsmiPopup->DropDownItems->Remove(m_tsmiTestNative);
	m_tsmiPopup->DropDownItems->Remove(m_tsmiSep);
	m_tsmiPopup->DropDownItems->Remove(m_tsmiAddEntries);

	ToolStripItemCollection^ tsMenu = m_host->MainWindow->ToolsMenu->DropDownItems;
	tsMenu->Remove(m_tsmiPopup);
}

void SamplePluginCppExt::TestAddEntries(Object^ sender, EventArgs^ e)
{
	UNREFERENCED_PARAMETER(sender);
	UNREFERENCED_PARAMETER(e);

	PwGroup^ pg = m_host->MainWindow->GetSelectedGroup();
	if(pg == nullptr)
	{
		::MessageBox(NULL, _T("You must first open a database!"),
			_T("Sample Plugin"), MB_ICONWARNING | MB_OK);
		return;
	}

	PwEntry^ pe = gcnew PwEntry(pg, true, true);
	pg->Entries->Add(pe);

	pe->Strings->Set(PwDefs::TitleField, gcnew ProtectedString(
		m_host->Database->MemoryProtection->ProtectTitle,
		_T("Sample Entry created by Sample Plugin (C++)")));

	m_host->MainWindow->UpdateEntryList(nullptr, true);
	m_host->MainWindow->UpdateUIState(true);
}

void SamplePluginCppExt::TestNative(Object^ sender, EventArgs^ e)
{
	UNREFERENCED_PARAMETER(sender);
	UNREFERENCED_PARAMETER(e);

	// STL can be used
	std::basic_string<TCHAR> strText = _T("This is a native ");
	strText += _T("Win32 message box!");

	// Set the main window as parent for the message box
	HWND hWndMain = reinterpret_cast<HWND>(m_host->MainWindow->Handle.ToPointer());

	// Display a native Win32 message box
	::MessageBox(hWndMain, strText.c_str(), _T("Sample Plugin"),
		MB_ICONINFORMATION | MB_OK);
}

} // Namespace SamplePluginCpp
