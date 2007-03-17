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
using System.IO;
using System.Net;

#if !KeePassLibSD
using System.Net.Cache;
#endif

namespace KeePassLib.Serialization
{
	public static class IOConnection
	{
#if !KeePassLibSD
		private static WebClient CreateWebClient(IOConnectionInfo ioc)
		{
			WebClient wc = new WebClient();

			wc.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

			if((ioc.UserName.Length > 0) || (ioc.Password.Length > 0))
				wc.Credentials = new NetworkCredential(ioc.UserName, ioc.Password);

			return wc;
		}

		public static FileOpenResult OpenRead(IOConnectionInfo ioc, out Stream s)
		{
			s = null;

			if(ioc.IsLocalFile()) return OpenReadLocal(ioc, out s);

			try
			{
				Stream sIn = CreateWebClient(ioc).OpenRead(new Uri(ioc.Url));
				if(sIn != null)
				{
					s = sIn;
					return FileOpenResult.Success;
				}
			}
			catch(Exception excp)
			{
				return new FileOpenResult(FileOpenResultCode.NoFileAccess,
					excp);
			}

			return new FileOpenResult(FileOpenResultCode.NoFileAccess, null);
		}
#else
		public static FileOpenResult OpenRead(IOConnectionInfo ioc, out Stream s)
		{
			return OpenReadLocal(ioc, out s);
		}
#endif

		private static FileOpenResult OpenReadLocal(IOConnectionInfo ioc, out Stream s)
		{
			s = null;

			FileStream fs = null;
			try
			{
				fs = new FileStream(ioc.Url, FileMode.Open, FileAccess.Read,
					FileShare.Read);
			}
			catch(FileNotFoundException fnfEx)
			{
				return new FileOpenResult(FileOpenResultCode.FileNotFound, fnfEx);
			}
			catch(IOException ioEx)
			{
				return new FileOpenResult(FileOpenResultCode.IOError, ioEx);
			}
			catch(Exception otherEx)
			{
				return new FileOpenResult(FileOpenResultCode.NoFileAccess, otherEx);
			}

			if(fs == null) return new FileOpenResult(FileOpenResultCode.NoFileAccess, null);

			s = fs;
			return FileOpenResult.Success;
		}

#if !KeePassLibSD
		public static FileSaveResult OpenWrite(IOConnectionInfo ioc, out Stream s)
		{
			s = null;

			if(ioc.IsLocalFile()) return OpenWriteLocal(ioc, out s);

			try
			{
				Stream sOut = CreateWebClient(ioc).OpenWrite(new Uri(ioc.Url));
				if(sOut != null)
				{
					s = sOut;
					return FileSaveResult.Success;
				}
			}
			catch(Exception excp)
			{
				return new FileSaveResult(FileSaveResultCode.FileCreationFailed,
					excp);
			}

			return new FileSaveResult(FileSaveResultCode.FileCreationFailed, null);
		}
#else
		public static FileSaveResult OpenWrite(IOConnectionInfo ioc, out Stream s)
		{
			return OpenWriteLocal(ioc, out s);
		}
#endif

		private static FileSaveResult OpenWriteLocal(IOConnectionInfo ioc, out Stream sSaveTo)
		{
			sSaveTo = null;

			FileStream fs = null;
			try
			{
				fs = new FileStream(ioc.Url, FileMode.Create,
					FileAccess.Write, FileShare.None);
			}
			catch(Exception fsEx)
			{
				return new FileSaveResult(FileSaveResultCode.FileCreationFailed, fsEx);
			}

			sSaveTo = fs;
			return FileSaveResult.Success;
		}
	}
}
