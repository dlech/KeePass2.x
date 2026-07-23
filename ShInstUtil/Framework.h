/*
  ShInstUtil
  Copyright (C) 2003-2026 Dominik Reichl <dominik.reichl@t-online.de>

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

#ifndef ___FRAMEWORK_H___
#define ___FRAMEWORK_H___

#pragma once

#define WIN32_LEAN_AND_MEAN

#include "TargetVer.h"

#include <cassert>
#include <cctype>
#include <cstdint>
#include <cstdlib>
#include <string>
#include <unordered_map>
#include <utility>
#include <vector>

#include <Windows.h>

#include <CommCtrl.h>
#include <ObjBase.h>
#include <ShellApi.h>
#include <ShlObj.h>
#include <ShObjIdl.h>
#include <TChar.h>
#include <VersionHelpers.h>

typedef std::basic_string<TCHAR> tstring;

#endif // ___FRAMEWORK_H___
