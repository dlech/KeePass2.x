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

#ifndef ___AES_KDF_H___
#define ___AES_KDF_H___

#pragma once

#include "../../Framework.h"
#include "../../Utility/AlignedBuffer.h"

#include <BCrypt.h>

class CAesKdf
{
public:
	CAesKdf(const uint8_t* pbSeed32);
	virtual ~CAesKdf();

	bool TransformHalf(uint8_t* pbData16, uint64_t uRounds);

private:
	CAlignedBufferUP m_upKeyObj;
	CAlignedBufferUP m_upIV;
	CAlignedBufferUP m_upZero;
	CAlignedBufferUP m_upBuf;

	BCRYPT_ALG_HANDLE m_hAlg;
	BCRYPT_KEY_HANDLE m_hKey;

	bool m_bReady;
};

KPL_API BOOL AesKdfTransformHalf(uint8_t* pbData16, const uint8_t* pbSeed32,
	uint64_t uRounds);
KPL_API uint64_t AesKdfTransformBenchmarkHalf(uint32_t uMilliseconds);

#endif // ___AES_KDF_H___
