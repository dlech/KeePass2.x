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

#pragma once

using namespace System;
using namespace System::Windows::Forms;

using namespace KeePass::Plugins;

namespace SamplePluginCpp
{
	public ref class SamplePluginCppExt : Plugin
	{
	public:
		virtual bool Initialize(IPluginHost^ host) override;
		virtual void Terminate() override;

	private:
		void TestAddEntries(Object^ sender, EventArgs^ e);
		void TestNative(Object^ sender, EventArgs^ e);

		IPluginHost^ m_host;

		ToolStripMenuItem^ m_tsmiPopup;
		ToolStripMenuItem^ m_tsmiAddEntries;
		ToolStripSeparator^ m_tsmiSep;
		ToolStripMenuItem^ m_tsmiTestNative;
	};
}
