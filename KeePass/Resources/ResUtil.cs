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

using System;

using KeePassLib;

namespace KeePass.Resources
{
	/// <summary>
	/// A class that contains several localization helper functions, like
	/// translating error codes to strings, etc.
	/// </summary>
	public static class ResUtil
	{
		/// <summary>
		/// Translate a <c>FileOpenResult</c> code to a string. The returned
		/// string is localized.
		/// </summary>
		/// <param name="fr"><c>FileOpenResult</c> code to be translated.</param>
		/// <returns>Localized string.</returns>
		public static string FileOpenResultToString(FileOpenResult fr)
		{
			string str;

			switch(fr.Code)
			{
				case FileOpenResultCode.Success:
					str = KPRes.Success;
					break;
				case FileOpenResultCode.FileNotFound:
					str = KPRes.FileNotFoundError;
					break;
				case FileOpenResultCode.InvalidFileStructure:
					str = KPRes.InvalidFileStructure;
					break;
				case FileOpenResultCode.InvalidFileSignature:
					str = KPRes.InvalidFileSignature;
					break;
				case FileOpenResultCode.IOError:
					str = KPRes.IOError;
					break;
				case FileOpenResultCode.NoFileAccess:
					str = KPRes.NoFileAccessRead;
					break;
				case FileOpenResultCode.UnknownEncryptionAlgorithm:
					str = KPRes.UnknownEncryptionAlgorithm;
					break;
				case FileOpenResultCode.UnknownError:
					str = KPRes.UnknownError;
					break;
				case FileOpenResultCode.UnknownFileVersion:
					str = KPRes.UnknownFileVersion;
					break;
				case FileOpenResultCode.SecurityException:
					str = KPRes.InternalSecurityException;
					break;
				case FileOpenResultCode.InvalidHeader:
					str = KPRes.InvalidHeader;
					break;
				case FileOpenResultCode.InvalidFileFormat:
					str = KPRes.InvalidFileFormat;
					break;
				default:
					str = KPRes.UnknownError;
					break;
			}

			if((fr.Message != null) && (fr.Message.Length > 0))
				str += "\r\n\r\n" + fr.Message;

			return str;
		}

		public static string FileSaveResultToString(FileSaveResult fsr)
		{
			string str;

			switch(fsr.Code)
			{
				case FileSaveResultCode.Success:
					str = KPRes.Success;
					break;
				case FileSaveResultCode.UnknownError:
					str = KPRes.UnknownError;
					break;
				case FileSaveResultCode.FileCreationFailed:
					str = KPRes.FileCreationError;
					break;
				case FileSaveResultCode.IOException:
					str = KPRes.IOError;
					break;
				case FileSaveResultCode.SecurityException:
					str = KPRes.InternalSecurityException;
					break;
				default:
					str = KPRes.UnknownError;
					break;
			}

			if((fsr.Message != null) && (fsr.Message.Length > 0))
				str += "\r\n\r\n" + fsr.Message;

			return str;
		}
	}
}
