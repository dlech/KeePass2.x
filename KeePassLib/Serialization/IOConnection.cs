/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Security.Cryptography.X509Certificates;

#if !KeePassLibSD
using System.Net.Cache;
using System.Net.Security;
#endif

namespace KeePassLib.Serialization
{
#if !KeePassLibSD
	public sealed class IOWebClient : WebClient
	{
		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest request = base.GetWebRequest(address);

			// WebDAV support
			if(request is HttpWebRequest)
			{
				request.PreAuthenticate = true; // Also auth GET
				if(request.Method == "POST") request.Method = "PUT";
			}

			return request;
		}
	}
#endif

	public static class IOConnection
	{
#if !KeePassLibSD
		// Allow self-signed certificates, expired certificates, etc.
		private static bool ValidateServerCertificate(object sender,
			X509Certificate certificate, X509Chain chain,
			SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		private static IOWebClient CreateWebClient(IOConnectionInfo ioc)
		{
			ServicePointManager.ServerCertificateValidationCallback =
				ValidateServerCertificate;

			IOWebClient wc = new IOWebClient();
			wc.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

			if((ioc.UserName.Length > 0) || (ioc.Password.Length > 0))
				wc.Credentials = new NetworkCredential(ioc.UserName, ioc.Password);

			return wc;
		}

		public static Stream OpenRead(IOConnectionInfo ioc)
		{
			if(ioc.IsLocalFile()) return OpenReadLocal(ioc);

			return CreateWebClient(ioc).OpenRead(new Uri(ioc.Path));
		}
#else
		public static Stream OpenRead(IOConnectionInfo ioc)
		{
			return OpenReadLocal(ioc);
		}
#endif

		private static Stream OpenReadLocal(IOConnectionInfo ioc)
		{
			return new FileStream(ioc.Path, FileMode.Open, FileAccess.Read,
				FileShare.Read);
		}

#if !KeePassLibSD
		public static Stream OpenWrite(IOConnectionInfo ioc)
		{
			if(ioc.IsLocalFile()) return OpenWriteLocal(ioc);

			return CreateWebClient(ioc).OpenWrite(new Uri(ioc.Path));
		}
#else
		public static Stream OpenWrite(IOConnectionInfo ioc)
		{
			return OpenWriteLocal(ioc);
		}
#endif

		private static Stream OpenWriteLocal(IOConnectionInfo ioc)
		{
			return new FileStream(ioc.Path, FileMode.Create, FileAccess.Write,
				FileShare.None);
		}
	}
}
