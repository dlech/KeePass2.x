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
#include <assert.h>
#include "KeePassNtv.h"

#include "AES/aesopt.h"

#ifdef _MANAGED
#pragma managed(push, off)
#endif

BOOL APIENTRY DllMain(HMODULE hModule, DWORD dwReason, LPVOID lpReserved)
{
	UNREFERENCED_PARAMETER(hModule);
	UNREFERENCED_PARAMETER(dwReason);
	UNREFERENCED_PARAMETER(lpReserved);

	return TRUE;
}

#define TK_ENC_PER_ROUND 128
#define AES_ENC_ROUND_001 aes_encrypt(pFirstBlock, pFirstBlock, &ctx); aes_encrypt(pSecondBlock, pSecondBlock, &ctx)

C_FN_SHARE BOOL TransformKey(__inout BYTE *pBuf256, __in const BYTE *pKey256,
	unsigned __int64 uRounds)
{
	assert(pBuf256 != NULL); if(pBuf256 == NULL) return FALSE;
	assert(pKey256 != NULL); if(pKey256 == NULL) return FALSE;

	aes_encrypt_ctx ctx;
	aes_encrypt_key256(pKey256, &ctx);

	const unsigned __int64 uFullRounds = uRounds / TK_ENC_PER_ROUND;
	const unsigned __int64 uRestRounds = uRounds % TK_ENC_PER_ROUND;

	BYTE *pFirstBlock = pBuf256;
	BYTE *pSecondBlock = &pBuf256[16];

	for(unsigned __int64 u = 0; u < uFullRounds; ++u)
	{
		for(int i = 0; i < 16; ++i)
		{
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
		}
	}

	for(unsigned __int64 uRest = 0; uRest < uRestRounds; ++uRest)
	{
		AES_ENC_ROUND_001;
	}

	return TRUE;
}

C_FN_SHARE BOOL TransformKeyTimed(__inout BYTE *pBuf256, __in const BYTE *pKey256,
	__out unsigned __int64 *puRounds, unsigned int uSeconds)
{
	assert(pBuf256 != NULL); if(pBuf256 == NULL) return FALSE;
	assert(pKey256 != NULL); if(pKey256 == NULL) return FALSE;

	aes_encrypt_ctx ctx;
	aes_encrypt_key256(pKey256, &ctx);

	BYTE *pFirstBlock = pBuf256;
	BYTE *pSecondBlock = &pBuf256[16];

	LARGE_INTEGER liFreq;
	QueryPerformanceFrequency(&liFreq);

	const LONGLONG llReqDiff = liFreq.QuadPart * uSeconds;
	LARGE_INTEGER liStart;
	LARGE_INTEGER liNow;
	QueryPerformanceCounter(&liStart);

	unsigned __int64 u;
	for(u = 0; ; ++u)
	{
		for(int i = 0; i < 16; ++i)
		{
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
			AES_ENC_ROUND_001;
		}

		QueryPerformanceCounter(&liNow);
		if((liNow.QuadPart - liStart.QuadPart) >= llReqDiff) break;
	}

	if(puRounds != NULL)
	{
		*puRounds = u * TK_ENC_PER_ROUND;
	}

	return TRUE;
}

#ifdef _MANAGED
#pragma managed(pop)
#endif
