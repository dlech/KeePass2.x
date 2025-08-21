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

#include "AesKdf.h"
#include "../../Utility/AppUtil.h"

#include <TimeApi.h>

#define AKC_FAIL { assert(false); return; }

constexpr size_t AkcBufferBlocks = 8192;
constexpr size_t AkcBufferSize = AkcBufferBlocks * 16;

CAesKdf::CAesKdf(const uint8_t* pbSeed32) :
	m_hAlg(NULL), m_hKey(NULL), m_bReady(false)
{
	if(pbSeed32 == nullptr) { assert(false); return; }

	if(!BCRYPT_SUCCESS(BCryptOpenAlgorithmProvider(&m_hAlg, BCRYPT_AES_ALGORITHM,
		nullptr, 0)))
		AKC_FAIL;
	if(m_hAlg == NULL) { BCryptCloseAlgorithmProvider(m_hAlg, 0); AKC_FAIL; }

	DWORD cbKeyObj = 0;
	ULONG cbResult = 0;
	if(!BCRYPT_SUCCESS(BCryptGetProperty(m_hAlg, BCRYPT_OBJECT_LENGTH,
		(PUCHAR)&cbKeyObj, sizeof(DWORD), &cbResult, 0)) || (cbKeyObj == 0))
		AKC_FAIL;

	m_upKeyObj.reset(new CAlignedBuffer(cbKeyObj, 16, true, true));
	if((m_upKeyObj.get() == nullptr) || (m_upKeyObj->Data() == nullptr)) AKC_FAIL;

	constexpr ULONG cbKeyBlobHeader = sizeof(BCRYPT_KEY_DATA_BLOB_HEADER);
	constexpr ULONG cbKeyBlob = cbKeyBlobHeader + 32;
	uint8_t vKeyBlob[cbKeyBlob] = { 0 };
	BCRYPT_KEY_DATA_BLOB_HEADER* pKeyBlob = (BCRYPT_KEY_DATA_BLOB_HEADER*)vKeyBlob;
	pKeyBlob->dwMagic = BCRYPT_KEY_DATA_BLOB_MAGIC;
	pKeyBlob->dwVersion = BCRYPT_KEY_DATA_BLOB_VERSION1;
	pKeyBlob->cbKeyData = 32;
	memcpy(vKeyBlob + cbKeyBlobHeader, pbSeed32, 32);

	const NTSTATUS s = BCryptImportKey(m_hAlg, NULL, BCRYPT_KEY_DATA_BLOB,
		&m_hKey, m_upKeyObj->Data(), cbKeyObj, vKeyBlob, cbKeyBlob, 0);
	SecureZeroMemory(vKeyBlob, cbKeyBlob);
	if(!BCRYPT_SUCCESS(s)) AKC_FAIL;

	LPCWSTR lpwCbc = BCRYPT_CHAIN_MODE_CBC;
	if(!BCRYPT_SUCCESS(BCryptSetProperty(m_hAlg, BCRYPT_CHAINING_MODE,
		(PUCHAR)lpwCbc, (ULONG)((wcslen(lpwCbc) + 1) * sizeof(WCHAR)), 0)))
		AKC_FAIL;

#ifdef _DEBUG
	DWORD cbKey = 0;
	assert(BCRYPT_SUCCESS(BCryptGetProperty(m_hKey, BCRYPT_KEY_LENGTH,
		(PUCHAR)&cbKey, sizeof(DWORD), &cbResult, 0)) && (cbKey == 256));

	BCRYPT_ALG_HANDLE hAlgRef = NULL;
	assert(BCRYPT_SUCCESS(BCryptGetProperty(m_hKey, BCRYPT_PROVIDER_HANDLE,
		(PUCHAR)&hAlgRef, sizeof(BCRYPT_ALG_HANDLE), &cbResult, 0)) &&
		(hAlgRef == m_hAlg));
#endif

	m_upIV.reset(new CAlignedBuffer(16, 16, true, true));
	if((m_upIV.get() == nullptr) || (m_upIV->Data() == nullptr)) AKC_FAIL;

	m_upZero.reset(new CAlignedBuffer(AkcBufferSize, 16, true, false));
	if((m_upZero.get() == nullptr) || (m_upZero->Data() == nullptr)) AKC_FAIL;

	m_upBuf.reset(new CAlignedBuffer(AkcBufferSize, 16, false, true));
	if((m_upBuf.get() == nullptr) || (m_upBuf->Data() == nullptr)) AKC_FAIL;

	m_bReady = true;
}

CAesKdf::~CAesKdf()
{
	if(m_hKey != NULL) { BCryptDestroyKey(m_hKey); m_hKey = NULL; }
	if(m_hAlg != NULL) { BCryptCloseAlgorithmProvider(m_hAlg, 0); m_hAlg = NULL; }
}

bool CAesKdf::TransformHalf(uint8_t* pbData16, uint64_t uRounds)
{
	if(!m_bReady) return false;

	uint8_t* pbIV = m_upIV->Data();
	if(pbData16 != nullptr) memcpy(pbIV, pbData16, 16);

	uint8_t* pbZero = m_upZero->Data();
	uint8_t* pbBuf = m_upBuf->Data();

	const bool bWine = AuxIsWine();

	while(uRounds != 0)
	{
		const uint64_t r = min(uRounds, (uint64_t)AkcBufferBlocks);
		const ULONG cb = (ULONG)r << 4;
		ULONG cbResult = 0;

		if(!BCRYPT_SUCCESS(BCryptEncrypt(m_hKey, pbZero, cb, nullptr, pbIV, 16,
			pbBuf, cb, &cbResult, 0)))
		{
			assert(false);
			return false;
		}

		assert(*(uint64_t*)pbZero == 0);
		assert(memcmp(pbIV, pbBuf + (cb - 16), 16) == 0);
		assert(cbResult == cb);

		// Workaround for https://bugs.winehq.org/show_bug.cgi?id=52457
		if(bWine) memcpy(pbIV, pbBuf + (cb - 16), 16);

		uRounds -= r;
	}

	if(pbData16 != nullptr) memcpy(pbData16, pbIV, 16);
	return true;
}

KPL_API BOOL AesKdfTransformHalf(uint8_t* pbData16, const uint8_t* pbSeed32,
	uint64_t uRounds)
{
	CAesKdf kdf(pbSeed32);
	return (kdf.TransformHalf(pbData16, uRounds) ? TRUE : FALSE);
}

KPL_API uint64_t AesKdfTransformBenchmarkHalf(uint32_t uMilliseconds)
{
	constexpr uint64_t cBlocks = AkcBufferBlocks;
	constexpr uint8_t vSeed32[32] = { 0 };

	CAesKdf kdf(vSeed32);
	uint64_t r = 0;

	const DWORD tStart = timeGetTime();
	while((timeGetTime() - tStart) <= uMilliseconds)
	{
		if(!kdf.TransformHalf(nullptr, cBlocks)) return 0;

		r += cBlocks;
		if(r < cBlocks) { r = UINT64_MAX; break; }
	}

	return r;
}
