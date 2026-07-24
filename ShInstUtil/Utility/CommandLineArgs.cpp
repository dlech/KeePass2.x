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

#include "CommandLineArgs.h"

using namespace std;

CCommandLineArgs::CCommandLineArgs()
{
	for(int i = 1; i < __argc; ++i)
	{
		LPCTSTR lp = __targv[i];
		if(lp == nullptr) { assert(false); continue; }
		if(lp[0] == _T('\0')) continue;

		tstring str(lp);

		if((str.size() >= 2) && (str[0] == _T('-')) && (str[1] == _T('-')))
			str = str.substr(2);
		else if((str[0] == _T('-')) || (str[0] == _T('/')))
			str = str.substr(1);
		else
		{
			m_vFiles.push_back(str);
			continue;
		}

		const size_t uSep = str.find_first_of(_T(":="));
		if(uSep == tstring::npos)
			m_vParams.push_back(make_pair(str, tstring()));
		else
			m_vParams.push_back(make_pair(str.substr(0, uSep), str.substr(uSep + 1)));
	}
}

LPCTSTR CCommandLineArgs::operator[](LPCTSTR lpName) const
{
	if(lpName == nullptr) { assert(false); return nullptr; }

	for(const auto& kvp : m_vParams)
	{
		if(_tcsicmp(kvp.first.c_str(), lpName) == 0)
			return kvp.second.c_str();
	}

	return nullptr;
}
