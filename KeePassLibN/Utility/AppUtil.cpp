/*
  KeePassLibN
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

#include "AppUtil.h"

#include <AclApi.h>
#include <TChar.h>

using namespace std;

static int g_iIsWine = 0;

static void AuxProtectProcessWithDaclCore(HANDLE hProcess, HANDLE hToken)
{
	DWORD cbTokenUser = 0;
	GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS::TokenUser, nullptr, 0,
		&cbTokenUser);
	if(cbTokenUser == 0) { assert(false); return; }

	vector<uint8_t> vTokenUser(cbTokenUser, 0);
	PTOKEN_USER pTokenUser = (PTOKEN_USER)vTokenUser.data();
	if(GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS::TokenUser, pTokenUser,
		cbTokenUser, &cbTokenUser) == FALSE)
	{
		assert(false);
		return;
	}

	PSID pSid = pTokenUser->User.Sid;
	if((pSid == nullptr) || (IsValidSid(pSid) == FALSE)) { assert(false); return; }

	const DWORD cbAcl = sizeof(ACL) + (sizeof(ACCESS_ALLOWED_ACE) - sizeof(DWORD)) +
		GetLengthSid(pSid);
	vector<uint8_t> vAcl(cbAcl, 0);
	PACL pAcl = (PACL)vAcl.data();

	if(InitializeAcl(pAcl, cbAcl, ACL_REVISION) == FALSE) { assert(false); return; }

	if(AddAccessAllowedAce(pAcl, ACL_REVISION, SYNCHRONIZE |
		PROCESS_QUERY_LIMITED_INFORMATION | PROCESS_TERMINATE, pSid) == FALSE)
	{
		assert(false);
		return;
	}

	if(SetSecurityInfo(hProcess, SE_KERNEL_OBJECT, DACL_SECURITY_INFORMATION,
		nullptr, nullptr, pAcl, nullptr) != ERROR_SUCCESS)
	{
		assert(false);
	}
}

KPL_API void AuxProtectProcessWithDacl()
{
	HANDLE hProcess = GetCurrentProcess();
	HANDLE hToken = NULL;

	if(OpenProcessToken(hProcess, TOKEN_QUERY, &hToken) != FALSE)
	{
		AuxProtectProcessWithDaclCore(hProcess, hToken);
		if(CloseHandle(hToken) == FALSE) { assert(false); }
	}
	else { assert(false); }
}

bool AuxIsWine()
{
	if(g_iIsWine == 0)
	{
		// https://gitlab.winehq.org/wine/wine/-/wikis/Developer-FAQ#how-can-i-detect-wine
		HMODULE h = LoadLibrary(_T("NTDLL.dll"));
		if(h != NULL)
		{
			g_iIsWine = ((GetProcAddress(h, "wine_get_version") != NULL) ? 1 : -1);
			FreeLibrary(h);
		}
		else { assert(false); g_iIsWine = -1; }
	}

	return (g_iIsWine > 0);
}
