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

#ifndef ___ALIGNED_BUFFER_H___
#define ___ALIGNED_BUFFER_H___

#pragma once

#include "../Framework.h"

class CAlignedBuffer
{
public:
	CAlignedBuffer(size_t cbSize, size_t cbAlignment, bool bZeroOnConstruct,
		bool bZeroOnDestruct);
	CAlignedBuffer(size_t cbSize, size_t cbAlignment, const uint8_t* pbInit,
		bool bZeroOnDestruct);
	virtual ~CAlignedBuffer();

	inline uint8_t* Data() const { return m_pb; }
	inline size_t Size() const { return m_cb; }

private:
	CAlignedBuffer();

	static uint8_t* AllocAlignedMemory(size_t cbSize, size_t cbAlignment);

	uint8_t* m_pb;
	size_t m_cb;

	bool m_bZeroOnDestruct;
};

typedef std::unique_ptr<CAlignedBuffer> CAlignedBufferUP;

#endif // ___ALIGNED_BUFFER_H___
