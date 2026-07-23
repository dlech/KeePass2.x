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

#ifndef ___SHINSTUTIL_H___
#define ___SHINSTUTIL_H___

#pragma once

#include "Framework.h"

bool IsDirectorySeparator(TCHAR tch);
tstring EnsureTerminatingSeparator(const tstring& str);
tstring GetFileDirectory(const tstring& str);

void UpdateNativeImage(bool bInstall);
void UpdateNativeImage(bool bInstall, bool bArm64);

void RegisterPreLoad(bool bRegister);

tstring GetKnownFolderPath(REFKNOWNFOLDERID rfid);
tstring GetDotNetInstallRoot(bool bArm64);
tstring GetKeePassExePath();

tstring FindNGen(bool bArm64);
void FindNGenRec(const tstring& strPath, tstring& strNGenPath, uint64_t& uVersion);

uint64_t GetFileVersion64(const tstring& strFilePath);

void CheckDotNetInstalled();

void CreateLink(const tstring& strLinkFilePath, const tstring& strTargetFilePath);
void UpdateLinks(bool bInstall, bool bAllUsers, bool bPrograms = true,
	bool bDesktop = true);

#endif // ___SHINSTUTIL_H___
