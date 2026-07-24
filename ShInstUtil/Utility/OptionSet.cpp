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

#include "OptionSet.h"

using namespace std;

COptionSet::COptionSet(LPCTSTR lpOptions)
{
	if((lpOptions == nullptr) || (lpOptions[0] == _T('\0'))) return;

	const tstring str = tstring(lpOptions) + _T(',');
	tstring strOption;

	for(TCHAR tch : str)
	{
		if(tch == _T(','))
		{
			if(strOption.size() == 0) continue;

			const bool b = (strOption[0] != _T('!'));
			if(!b)
			{
				strOption = strOption.substr(1);
				if(strOption.size() == 0) continue;
			}

			m_m[strOption] = b;
			strOption.clear();
		}
		else if(!_istspace(tch)) strOption += tch;
	}
}

bool COptionSet::Get(LPCTSTR lpName, bool bDefault) const
{
	if(lpName == nullptr) { assert(false); return bDefault; }

	const auto it = m_m.find(tstring(lpName));
	return ((it != m_m.end()) ? it->second : bDefault);
}
