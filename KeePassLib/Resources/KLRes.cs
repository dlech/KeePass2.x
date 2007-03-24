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
using System.Collections.Generic;
using System.Text;

namespace KeePassLib.Resources
{
	public static class KLRes
	{
		public const string CryptoStreamFailed =
			"Failed to initialize encryption/decryption stream!";
		public const string InvalidCompositeKey =
			"The composite key is invalid!";
		public const string InvalidCompositeKeyHint =
			"Make sure the composite key is correct and try again.";
		public const string FileUnknownCipher =
			"The file encrypted using an unknown encryption algorithm!";
		public const string FileSigInvalid =
			"The file signature is invalid. Either the file isn't a " +
			"KeePass database file at all or it is corrupted.";
		public const string FileVersionUnknown =
			"Unknown file version!";
		public const string FileHeaderEndEarly =
			"The file header is corrupted! Some header data was " +
			"declared but is not present.";
		public const string UnknownHeaderID = "Unknown header ID!";
		public const string FileUnknownCompression =
			"The file is compressed using an unknown compression algorithm!";
		public const string MasterSeedLengthInvalid =
			"The length of the master key seed is invalid!";
		public const string FinalKeyCreationFailed =
			"Failed to create the final encryption/decryption key!";
		public const string OldFormat =
			"The selected file appears to be an old format";
		public const string FatalError = "Fatal Error";
		public const string FatalErrorText = "A fatal error has occured!";
		public const string FileLoadFailed = "Failed to load the specified file!";
		public const string FileSaveFailed =
			"Failed to save the current database to the specified location!";
		public const string ErrorFeedbackRequest =
			"An extended error report has been copied to the clipboard. " +
			"Please send it to the KeePass developers.";
	}
}
