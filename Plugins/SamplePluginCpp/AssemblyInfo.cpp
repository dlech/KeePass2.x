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

using namespace System;
using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Security::Permissions;

// Assembly definitions
[assembly:AssemblyTitleAttribute("SamplePluginCpp")];
[assembly:AssemblyDescriptionAttribute("A sample plugin for developers that demonstrates how to add menu items, password groups and entries.")];
[assembly:AssemblyConfigurationAttribute("")];
[assembly:AssemblyCompanyAttribute("Dominik Reichl")];
[assembly:AssemblyProductAttribute("KeePass Plugin")];
[assembly:AssemblyCopyrightAttribute("Copyright © 2006-2007 Dominik Reichl")];
[assembly:AssemblyTrademarkAttribute("")];
[assembly:AssemblyCultureAttribute("")];

// Assembly version
[assembly:AssemblyVersionAttribute("2.0.2.*")];

// Assembly COM settings
[assembly:ComVisible(false)];

// Assembly CLS compliance
[assembly:CLSCompliantAttribute(true)];

// Require unmanaged code permission for the complete assembly
[assembly:SecurityPermission(SecurityAction::RequestMinimum, UnmanagedCode = true)];
