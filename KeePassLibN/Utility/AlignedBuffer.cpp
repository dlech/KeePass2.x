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

#include "AlignedBuffer.h"

CAlignedBuffer::CAlignedBuffer() :
	m_pb(nullptr), m_cb(0), m_bZeroOnDestruct(false)
{
}

CAlignedBuffer::CAlignedBuffer(size_t cbSize, size_t cbAlignment,
	bool bZeroOnConstruct, bool bZeroOnDestruct) :
	m_cb(cbSize), m_bZeroOnDestruct(bZeroOnDestruct)
{
	m_pb = AllocAlignedMemory(cbSize, cbAlignment);

	if(bZeroOnConstruct && (m_pb != nullptr)) ZeroMemory(m_pb, cbSize);
}

CAlignedBuffer::CAlignedBuffer(size_t cbSize, size_t cbAlignment,
	const uint8_t* pbInit, bool bZeroOnDestruct) :
	m_cb(cbSize), m_bZeroOnDestruct(bZeroOnDestruct)
{
	m_pb = AllocAlignedMemory(cbSize, cbAlignment);

	if((pbInit != nullptr) && (m_pb != nullptr)) memcpy(m_pb, pbInit, cbSize);
}

CAlignedBuffer::~CAlignedBuffer()
{
	if(m_pb != nullptr)
	{
		if(m_bZeroOnDestruct) SecureZeroMemory(m_pb, m_cb);

		_aligned_free(m_pb);
		m_pb = nullptr;
	}
}

uint8_t* CAlignedBuffer::AllocAlignedMemory(size_t cbSize, size_t cbAlignment)
{
	assert(cbSize > 0);
	assert((cbAlignment & (cbAlignment - 1)) == 0); // Power of 2

	uint8_t* pb = (uint8_t*)_aligned_malloc(cbSize, cbAlignment);
	assert(((size_t)pb & (cbAlignment - 1)) == 0);

	return pb;
}
