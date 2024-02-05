/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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

#include "ShInstUtil.h"

#pragma warning(push)
#pragma warning(disable: 4996) // SCL warning
#include <boost/smart_ptr.hpp>
#include <boost/algorithm/string/trim.hpp>
#pragma warning(pop)

static const std_string g_strNGenInstall = _T("ngen_install");
static const std_string g_strNGenUninstall = _T("ngen_uninstall");
static const std_string g_strNetCheck = _T("net_check");
static const std_string g_strPreLoadRegister = _T("preload_register");
static const std_string g_strPreLoadUnregister = _T("preload_unregister");

static LPCTSTR g_lpPathTrimChars = _T("\"' \t\r\n");

int WINAPI _tWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance,
	LPTSTR lpCmdLine, int nCmdShow)
{
	UNREFERENCED_PARAMETER(hInstance);
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);
	UNREFERENCED_PARAMETER(nCmdShow);

	INITCOMMONCONTROLSEX icc;
	ZeroMemory(&icc, sizeof(INITCOMMONCONTROLSEX));
	icc.dwSize = sizeof(INITCOMMONCONTROLSEX);
	icc.dwICC = ICC_STANDARD_CLASSES;
	InitCommonControlsEx(&icc);

	std_string strCmdLine = GetCommandLine();
	boost::trim_if(strCmdLine, boost::is_any_of(g_lpPathTrimChars));
	std::transform(strCmdLine.begin(), strCmdLine.end(), strCmdLine.begin(), _totlower);

	if(StrEndsWith(strCmdLine, g_strNGenInstall))
	{
		UpdateNativeImage(false);
		Sleep(200);
		UpdateNativeImage(true);
	}

	if(StrEndsWith(strCmdLine, g_strNGenUninstall))
		UpdateNativeImage(false);

	if(StrEndsWith(strCmdLine, g_strPreLoadRegister))
	{
		RegisterPreLoad(false); // Remove old value in 32-bit reg. view on 64-bit systems
		RegisterPreLoad(true);
	}

	if(StrEndsWith(strCmdLine, g_strPreLoadUnregister))
		RegisterPreLoad(false);

	if(StrEndsWith(strCmdLine, g_strNetCheck))
		CheckDotNetInstalled();

	return 0;
}

bool StrEndsWith(const std_string& strText, const std_string& strEnd)
{
	if(strEnd.size() == 0) return true;
	if(strEnd.size() > strText.size()) return false;
	return (strText.substr(strText.size() - strEnd.size()) == strEnd);
}

void EnsureTerminatingSeparator(std_string& strPath)
{
	if(strPath.size() == 0) return;
	if(strPath[strPath.size() - 1] == _T('\\')) return;

	strPath += _T("\\");
}

void UpdateNativeImage(bool bInstall)
{
	const std_string strNGen = FindNGen();
	if(strNGen.size() == 0) return;

	const std_string strKeePassExe = GetKeePassExePath();
	if(strKeePassExe.size() == 0) return;

	std_string strParam = (bInstall ? _T("") : _T("un"));
	strParam += (_T("install \"") + strKeePassExe) + _T("\"");

	SHELLEXECUTEINFO sei;
	ZeroMemory(&sei, sizeof(SHELLEXECUTEINFO));
	sei.cbSize = sizeof(SHELLEXECUTEINFO);
	sei.fMask = SEE_MASK_NOCLOSEPROCESS;
	sei.lpVerb = _T("open");
	sei.lpFile = strNGen.c_str();
	sei.lpParameters = strParam.c_str();
	sei.nShow = SW_HIDE;
	ShellExecuteEx(&sei);

	if(sei.hProcess != NULL)
	{
		WaitForSingleObject(sei.hProcess, 16000);
		CloseHandle(sei.hProcess);
	}
}

void RegisterPreLoad(bool bRegister)
{
	LPCTSTR lpKey = _T("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
	LPCTSTR lpName = _T("KeePass 2 PreLoad");

	const std_string strExe = GetKeePassExePath();
	if(strExe.size() == 0) return;
	const std_string strValue = (_T("\"") + strExe) + _T("\" --preload");

	HKEY hRoot = HKEY_LOCAL_MACHINE;
	HKEY h = NULL;
	LSTATUS l;

	if(bRegister)
	{
		l = RegOpenKeyEx(hRoot, lpKey, 0, KEY_WRITE | KEY_WOW64_64KEY, &h);
		if((l != ERROR_SUCCESS) || (h == NULL))
			l = RegCreateKeyEx(hRoot, lpKey, 0, NULL, 0, KEY_WRITE |
				KEY_WOW64_64KEY, NULL, &h, NULL);
		if((l == ERROR_SUCCESS) && (h != NULL))
		{
			RegSetValueEx(h, lpName, 0, REG_SZ, (const BYTE*)strValue.c_str(),
				static_cast<DWORD>((strValue.size() + 1) * sizeof(TCHAR)));
			RegCloseKey(h);
		}
	}
	else // Unregister
	{
		for(size_t i = 0; i < 2; ++i)
		{
			l = RegOpenKeyEx(hRoot, lpKey, 0, KEY_WRITE |
				((i == 0) ? KEY_WOW64_64KEY : KEY_WOW64_32KEY), &h);
			if((l == ERROR_SUCCESS) && (h != NULL))
			{
				RegDeleteValue(h, lpName);
				RegCloseKey(h);
				h = NULL;
			}
		}
	}
}

std_string GetNetInstallRoot()
{
	std_string str;

	HKEY hNet = NULL;
	LONG lRes = RegOpenKeyEx(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\.NETFramework"),
		0, KEY_READ | KEY_WOW64_64KEY, &hNet);
	if((lRes != ERROR_SUCCESS) || (hNet == NULL)) return str;

	const DWORD cbData = 2050;
	BYTE pbData[cbData];
	ZeroMemory(pbData, cbData * sizeof(BYTE));
	DWORD dwData = cbData - 2;
	lRes = RegQueryValueEx(hNet, _T("InstallRoot"), NULL, NULL, pbData, &dwData);
	if(lRes == ERROR_SUCCESS) str = (LPCTSTR)(LPTSTR)pbData;

	RegCloseKey(hNet);
	return str;
}

std_string GetKeePassExePath()
{
	const DWORD cbData = 2050;
	TCHAR tszName[cbData];
	ZeroMemory(tszName, cbData * sizeof(TCHAR));

	GetModuleFileName(NULL, tszName, cbData - 2);

	for(int i = static_cast<int>(_tcslen(tszName)) - 1; i >= 0; --i)
	{
		if(tszName[i] == _T('\\')) break;
		else tszName[i] = 0;
	}

	std_string strPath = tszName;
	boost::trim_if(strPath, boost::is_any_of(g_lpPathTrimChars));
	if(strPath.size() == 0) return strPath;

	return (strPath + _T("KeePass.exe"));
}

std_string FindNGen()
{
	std_string strNGen;

	std_string strRoot = GetNetInstallRoot();
	if(strRoot.size() == 0) return strNGen;
	EnsureTerminatingSeparator(strRoot);

	ULONGLONG ullVersion = 0;
	FindNGenRec(strRoot, strNGen, ullVersion);

	return strNGen;
}

void FindNGenRec(const std_string& strPath, std_string& strNGenPath,
	ULONGLONG& ullVersion)
{
	const std_string strSearch = strPath + _T("*.*");
	const std_string strNGen = _T("ngen.exe");

	WIN32_FIND_DATA wfd;
	ZeroMemory(&wfd, sizeof(WIN32_FIND_DATA));
	HANDLE hFind = FindFirstFile(strSearch.c_str(), &wfd);
	if(hFind == INVALID_HANDLE_VALUE) return;

	do
	{
		if((wfd.cFileName[0] == _T('\0')) || (_tcsicmp(wfd.cFileName, _T(".")) == 0) ||
			(_tcsicmp(wfd.cFileName, _T("..")) == 0)) { }
		else if((wfd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
			FindNGenRec((strPath + wfd.cFileName) + _T("\\"), strNGenPath, ullVersion);
		else if(_tcsicmp(wfd.cFileName, strNGen.c_str()) == 0)
		{
			const std_string strFullPath = strPath + strNGen;
			const ULONGLONG ullThisVer = SiuGetFileVersion(strFullPath);
			if(ullThisVer >= ullVersion)
			{
				strNGenPath = strFullPath;
				ullVersion = ullThisVer;
			}
		}
	}
	while(FindNextFile(hFind, &wfd) != FALSE);

	FindClose(hFind);
}

ULONGLONG SiuGetFileVersion(const std_string& strFilePath)
{
	DWORD dwDummy = 0;
	const DWORD dwVerSize = GetFileVersionInfoSize(
		strFilePath.c_str(), &dwDummy);
	if(dwVerSize == 0) return 0;

	boost::scoped_array<BYTE> vVerInfo(new BYTE[dwVerSize]);
	if(vVerInfo.get() == NULL) return 0; // Out of memory

	if(GetFileVersionInfo(strFilePath.c_str(), 0, dwVerSize,
		vVerInfo.get()) == FALSE) return 0;

	VS_FIXEDFILEINFO* pFileInfo = NULL;
	UINT uFixedInfoLen = 0;
	if(VerQueryValue(vVerInfo.get(), _T("\\"), (LPVOID*)&pFileInfo,
		&uFixedInfoLen) == FALSE) return 0;
	if(pFileInfo == NULL) return 0;

	return ((static_cast<ULONGLONG>(pFileInfo->dwFileVersionMS) <<
		32) | static_cast<ULONGLONG>(pFileInfo->dwFileVersionLS));
}

void CheckDotNetInstalled()
{
	OSVERSIONINFO osv;
	ZeroMemory(&osv, sizeof(OSVERSIONINFO));
	osv.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
	GetVersionEx(&osv);
	if(osv.dwMajorVersion >= 6) return; // .NET ships with Vista and higher

	const std_string strNGen = FindNGen();
	if(strNGen.size() == 0)
	{
		std_string strMsg = _T("KeePass 2.x requires the Microsoft .NET Framework 2.0 or higher. ");
		strMsg += _T("This framework currently does not seem to be installed ");
		strMsg += _T("on your computer. Without this framework, KeePass will not run.\r\n\r\n");
		strMsg += _T("The Microsoft .NET Framework is available as free download from the ");
		strMsg += _T("Microsoft website.\r\n\r\n");
		strMsg += _T("Do you want to visit the Microsoft website now?");

		const int nRes = MessageBox(NULL, strMsg.c_str(), _T("KeePass Setup"),
			MB_ICONQUESTION | MB_YESNO);
		if(nRes == IDYES)
		{
			SHELLEXECUTEINFO sei;
			ZeroMemory(&sei, sizeof(SHELLEXECUTEINFO));
			sei.cbSize = sizeof(SHELLEXECUTEINFO);
			sei.lpVerb = _T("open");
			sei.lpFile = _T("https://msdn.microsoft.com/en-us/netframework/aa569263.aspx");
			sei.nShow = SW_SHOW;
			ShellExecuteEx(&sei);
		}
	}
}
