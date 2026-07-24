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

#include "ShInstUtil.h"

#include "Utility/CommandLineArgs.h"
#include "Utility/OptionSet.h"

#pragma comment(lib, "ComCtl32.lib")
#pragma comment(lib, "Version.lib")

#pragma comment(linker, "/manifestdependency:\"type='win32' " \
	"name='Microsoft.Windows.Common-Controls' version='6.0.0.0' " \
	"processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")

using namespace std;

int APIENTRY _tWinMain(_In_ HINSTANCE hInstance, _In_opt_ HINSTANCE hPrevInstance,
	_In_ LPTSTR lpCmdLine, _In_ int nCmdShow)
{
	UNREFERENCED_PARAMETER(hInstance);
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);
	UNREFERENCED_PARAMETER(nCmdShow);

	const HRESULT hrCom = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);

	INITCOMMONCONTROLSEX icc;
	ZeroMemory(&icc, sizeof(INITCOMMONCONTROLSEX));
	icc.dwSize = sizeof(INITCOMMONCONTROLSEX);
	icc.dwICC = ICC_STANDARD_CLASSES;
	InitCommonControlsEx(&icc);

	CCommandLineArgs cla;

	LPCTSTR lpAllUsers = cla[_T("AllUsers")];
	const bool bAllUsers = ((lpAllUsers != nullptr) &&
		((_tcscmp(lpAllUsers, _T("1")) == 0) || (_tcscmp(lpAllUsers, _T("2")) == 0)));

	LPCTSTR lpCommand = cla[_T("C")];
	if(lpCommand == nullptr) { }
	else if(_tcsicmp(lpCommand, _T("NGenInstall")) == 0)
		UpdateNativeImage(true);
	else if(_tcsicmp(lpCommand, _T("NGenUninstall")) == 0)
		UpdateNativeImage(false);
	else if(_tcsicmp(lpCommand, _T("PreLoadRegister")) == 0)
		RegisterPreLoad(true);
	else if(_tcsicmp(lpCommand, _T("PreLoadUnregister")) == 0)
		RegisterPreLoad(false);
	else if(_tcsicmp(lpCommand, _T("DotNetCheck")) == 0)
		CheckDotNetInstalled();
	else if(_tcsicmp(lpCommand, _T("MsiInstall")) == 0)
	{
		const COptionSet os(cla[_T("KpsOptions")]);

		UpdateLinks(true, bAllUsers, os.Get(_T("StartMenuIcons"), true),
			os.Get(_T("DesktopIcon"), true));
		if(bAllUsers)
		{
			if(os.Get(_T("NGen"), true)) UpdateNativeImage(true);
			if(os.Get(_T("PreLoad"), true)) RegisterPreLoad(true);
		}
	}
	else if(_tcsicmp(lpCommand, _T("MsiUninstall")) == 0)
	{
		UpdateLinks(false, bAllUsers);
		if(bAllUsers)
		{
			UpdateNativeImage(false);
			RegisterPreLoad(false);
		}
	}

	if(SUCCEEDED(hrCom)) CoUninitialize();

	return 0;
}

bool IsDirectorySeparator(TCHAR tch)
{
	return ((tch == _T('\\')) || (tch == _T('/')));
}

tstring EnsureTerminatingSeparator(const tstring& str)
{
	if(str.size() == 0) { assert(false); return tstring(); }

	if(IsDirectorySeparator(str[str.size() - 1])) return str;

	return (str + _T("\\"));
}

tstring GetFileDirectory(const tstring& str)
{
	if(str.size() == 0) { assert(false); return tstring(); }

	for(size_t i = str.size() - 1; i != 0; --i)
	{
		if(IsDirectorySeparator(str[i])) return str.substr(0, i);
	}

	return tstring();
}

void UpdateNativeImage(bool bInstall)
{
	if(bInstall)
	{
		UpdateNativeImage(false);
		Sleep(200);
	}

	UpdateNativeImage(bInstall, false);
	UpdateNativeImage(bInstall, true);
}

void UpdateNativeImage(bool bInstall, bool bArm64)
{
	const tstring strNGen = FindNGen(bArm64);
	if(strNGen.size() == 0) return;

	const tstring strKeePass = GetKeePassExePath();
	if(strKeePass.size() == 0) { assert(false); return; }

	// When updating using the MSI file and changing the installation directory,
	// the 'Uninstall' custom action is not executed; thus, uninstall *all* native
	// KeePass images
	assert(strKeePass.find(_T("\\KeePass.exe")) != tstring::npos);
	const tstring strArgs = (bInstall ? (_T("install \"") + strKeePass + _T("\"")) :
		tstring(_T("uninstall KeePass")));

	SHELLEXECUTEINFO sei;
	ZeroMemory(&sei, sizeof(SHELLEXECUTEINFO));
	sei.cbSize = sizeof(SHELLEXECUTEINFO);
	sei.fMask = SEE_MASK_NOCLOSEPROCESS;
	sei.lpVerb = _T("open");
	sei.lpFile = strNGen.c_str();
	sei.lpParameters = strArgs.c_str();
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
	HKEY hRoot = HKEY_LOCAL_MACHINE;
	LPCTSTR lpKey = _T("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
	LPCTSTR lpName = _T("KeePass 2 PreLoad");

	const tstring strExe = GetKeePassExePath();
	if(strExe.size() == 0) return;
	const tstring strValue = _T("\"") + strExe + _T("\" --preload");

	if(bRegister)
	{
		HKEY h = NULL;
		LSTATUS r = RegOpenKeyEx(hRoot, lpKey, 0, KEY_WRITE | KEY_WOW64_64KEY, &h);
		if((r != ERROR_SUCCESS) || (h == NULL))
			r = RegCreateKeyEx(hRoot, lpKey, 0, nullptr, 0, KEY_WRITE | KEY_WOW64_64KEY,
				nullptr, &h, nullptr);
		if((r == ERROR_SUCCESS) && (h != NULL))
		{
			RegSetValueEx(h, lpName, 0, REG_SZ, reinterpret_cast<const BYTE*>(
				strValue.c_str()), static_cast<DWORD>((strValue.size() + 1) * sizeof(TCHAR)));
			RegCloseKey(h);
		}
	}
	else // Unregister
	{
		for(int i = 0; i < 2; ++i)
		{
			HKEY h = NULL;
			const LSTATUS r = RegOpenKeyEx(hRoot, lpKey, 0, KEY_WRITE |
				((i == 0) ? KEY_WOW64_64KEY : KEY_WOW64_32KEY), &h);
			if((r == ERROR_SUCCESS) && (h != NULL))
			{
				RegDeleteValue(h, lpName);
				RegCloseKey(h);
			}
		}
	}
}

tstring GetKnownFolderPath(REFKNOWNFOLDERID rfid)
{
	static_assert((sizeof(TCHAR) == sizeof(WCHAR)), "Unsupported TCHAR size!");
	tstring str;

	PWSTR psz = nullptr;
	if(SUCCEEDED(SHGetKnownFolderPath(rfid, 0, NULL, &psz)) && (psz != nullptr))
		str = psz;
	if(psz != nullptr) CoTaskMemFree(psz);

	return str;
}

tstring GetDotNetInstallRoot(bool bArm64)
{
	HKEY h = NULL;
	LSTATUS r = RegOpenKeyEx(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\.NETFramework"),
		0, KEY_READ | KEY_WOW64_64KEY, &h);
	if((r != ERROR_SUCCESS) || (h == NULL)) return tstring();

	LPCTSTR lpName = (bArm64 ? _T("InstallRootArm64") : _T("InstallRoot"));

	constexpr DWORD cbBuffer = MAX_PATH * 4 * sizeof(TCHAR);
	BYTE ab[cbBuffer] = { 0 };
	DWORD cb = cbBuffer - sizeof(TCHAR);
	r = RegQueryValueEx(h, lpName, nullptr, nullptr, ab, &cb);

	RegCloseKey(h);

	return ((r == ERROR_SUCCESS) ? tstring(reinterpret_cast<LPCTSTR>(ab)) : tstring());
}

tstring GetKeePassExePath()
{
	constexpr DWORD ccBuffer = MAX_PATH * 4;
	TCHAR tsz[ccBuffer] = { _T('\0') };

	const DWORD cc = GetModuleFileName(NULL, tsz, ccBuffer);
	if((cc == 0) || (cc == ccBuffer)) { assert(false); return tstring(); }
	assert(cc == _tcslen(tsz));

	tstring str = GetFileDirectory(tstring(tsz));
	if(str.size() != 0) str = EnsureTerminatingSeparator(str);
	else { assert(false); }

	return (str + _T("KeePass.exe"));
}

tstring FindNGen(bool bArm64)
{
	tstring strRoot = GetDotNetInstallRoot(bArm64);
	if(strRoot.size() == 0) return tstring();
	strRoot = EnsureTerminatingSeparator(strRoot);

	tstring strNGenPath;
	uint64_t uVersion = 0;
	FindNGenRec(strRoot, strNGenPath, uVersion);

	return strNGenPath;
}

void FindNGenRec(const tstring& strPath, tstring& strNGenPath, uint64_t& uVersion)
{
	const tstring strSearch = strPath + _T("*.*");

	WIN32_FIND_DATA wfd;
	ZeroMemory(&wfd, sizeof(WIN32_FIND_DATA));
	HANDLE hFind = FindFirstFile(strSearch.c_str(), &wfd);
	if(hFind == INVALID_HANDLE_VALUE) return;

	do
	{
		LPCTSTR lpName = wfd.cFileName;

		if((lpName[0] == _T('\0')) || (_tcscmp(lpName, _T(".")) == 0) ||
			(_tcscmp(lpName, _T("..")) == 0)) { }
		else if((wfd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
			FindNGenRec((strPath + lpName) + _T("\\"), strNGenPath, uVersion);
		else if(_tcsicmp(lpName, _T("ngen.exe")) == 0)
		{
			const tstring strFullPath = strPath + lpName;
			const uint64_t uThis = GetFileVersion64(strFullPath);
			if(uThis >= uVersion)
			{
				strNGenPath = strFullPath;
				uVersion = uThis;
			}
		}
	}
	while(FindNextFile(hFind, &wfd) != FALSE);

	FindClose(hFind);
}

uint64_t GetFileVersion64(const tstring& strFilePath)
{
	DWORD dwHandle = 0;
	const DWORD cb = GetFileVersionInfoSize(strFilePath.c_str(), &dwHandle);
	if(cb == 0) return 0;

	vector<uint8_t> v(cb);
	if(GetFileVersionInfo(strFilePath.c_str(), dwHandle, cb, v.data()) == FALSE)
		return 0;

	VS_FIXEDFILEINFO* pffi = nullptr;
	UINT cbFixed = 0;
	if((VerQueryValue(v.data(), _T("\\"), reinterpret_cast<LPVOID*>(&pffi),
		&cbFixed) == FALSE) || (pffi == nullptr))
		return 0;
	assert(cbFixed == sizeof(VS_FIXEDFILEINFO));

	return ((static_cast<uint64_t>(pffi->dwFileVersionMS) << 32) | pffi->dwFileVersionLS);
}

void CheckDotNetInstalled()
{
	if(IsWindows7OrGreater()) return; // .NET >= 3.5 is included in Windows 7 and later
	if(FindNGen(false).size() != 0) return;

	tstring strMsg = _T("KeePass 2.x requires the Microsoft .NET Framework 3.5 or later. ");
	strMsg += _T("This framework currently does not seem to be installed on your ");
	strMsg += _T("computer. Without this framework, KeePass will not run.\r\n\r\n");
	strMsg += _T("The Microsoft .NET Framework is available as free download from the ");
	strMsg += _T("Microsoft website.\r\n\r\n");
	strMsg += _T("Do you want to visit the Microsoft website now?");

	const int r = MessageBox(NULL, strMsg.c_str(), _T("KeePass Setup"),
		MB_ICONQUESTION | MB_YESNO);
	if(r == IDYES)
	{
		SHELLEXECUTEINFO sei;
		ZeroMemory(&sei, sizeof(SHELLEXECUTEINFO));
		sei.cbSize = sizeof(SHELLEXECUTEINFO);
		sei.lpVerb = _T("open");
		sei.lpFile = _T("https://dotnet.microsoft.com/en-us/download/dotnet-framework");
		sei.nShow = SW_SHOW;
		ShellExecuteEx(&sei);
	}
}

void CreateLink(const tstring& strLinkFilePath, const tstring& strTargetFilePath)
{
	if(strLinkFilePath.size() == 0) { assert(false); return; }
	if(strTargetFilePath.size() == 0) { assert(false); return; }

	IShellLink* psl = nullptr;
	if(FAILED(CoCreateInstance(CLSID_ShellLink, nullptr, CLSCTX_INPROC_SERVER,
		IID_PPV_ARGS(&psl))) || (psl == nullptr))
		return;

	psl->SetPath(strTargetFilePath.c_str());

	const tstring strWD = GetFileDirectory(strTargetFilePath);
	psl->SetWorkingDirectory(strWD.c_str());

	IPersistFile* ppf = nullptr;
	if(SUCCEEDED(psl->QueryInterface(IID_PPV_ARGS(&ppf))) && (ppf != nullptr))
	{
		static_assert((sizeof(OLECHAR) == sizeof(TCHAR)), "Unsupported OLECHAR size!");
		ppf->Save(strLinkFilePath.c_str(), TRUE);

		ppf->Release();
	}
	else { assert(false); }

	psl->Release();
}

void UpdateLinks(bool bInstall, bool bAllUsers, bool bPrograms, bool bDesktop)
{
	const tstring strPrograms = GetKnownFolderPath(bAllUsers ?
		FOLDERID_CommonPrograms : FOLDERID_Programs);
	assert(strPrograms.size() != 0);
	const tstring strDesktop = GetKnownFolderPath(bAllUsers ?
		FOLDERID_PublicDesktop : FOLDERID_Desktop);
	assert(strDesktop.size() != 0);

	const tstring strExe = GetKeePassExePath();
	const tstring strChm = EnsureTerminatingSeparator(GetFileDirectory(strExe)) +
		_T("KeePass.chm");

	for(int i = 1; i <= 2; ++i)
	{
		const tstring strSfx = ((i == 1) ? _T("") : _T(" 2"));
		const bool bCreate = (bInstall && (i == 2));
		const bool bDelete = ((!bInstall && (i == 2)) || (bInstall && (i == 1)));

		if(bPrograms && (strPrograms.size() != 0))
		{
			const tstring strLnkDir = EnsureTerminatingSeparator(strPrograms) +
				_T("KeePass") + strSfx;
			const tstring strLnkExe = EnsureTerminatingSeparator(strLnkDir) +
				_T("KeePass.lnk");
			const tstring strLnkChm = EnsureTerminatingSeparator(strLnkDir) +
				_T("KeePass User Manual.lnk");

			if(bCreate)
			{
				SHCreateDirectoryEx(NULL, strLnkDir.c_str(), nullptr);
				CreateLink(strLnkExe, strExe);
				CreateLink(strLnkChm, strChm);
			}
			if(bDelete)
			{
				DeleteFile(strLnkExe.c_str());
				DeleteFile(strLnkChm.c_str());
				RemoveDirectory(strLnkDir.c_str());
			}
		}

		if(bDesktop && (strDesktop.size() != 0))
		{
			const tstring strLnkExe = EnsureTerminatingSeparator(strDesktop) +
				_T("KeePass") + strSfx + _T(".lnk");

			if(bCreate)
			{
				SHCreateDirectoryEx(NULL, strDesktop.c_str(), nullptr);
				CreateLink(strLnkExe, strExe);
			}
			if(bDelete) DeleteFile(strLnkExe.c_str());
		}
	}
}
