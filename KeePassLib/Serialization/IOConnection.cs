/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;

#if !KeePassLibSD
using System.Net.Cache;
using System.Net.Security;
#endif

using KeePassLib.Utility;

namespace KeePassLib.Serialization
{
#if !KeePassLibSD
	public sealed class IOWebClient : WebClient
	{
		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest request = base.GetWebRequest(address);
			IOConnection.ConfigureWebRequest(request);
			return request;
		}
	}
#endif

	public static class IOConnection
	{
		// Web request methods
		public const string WrmDeleteFile = "DELETEFILE";
		public const string WrmMoveFile = "MOVEFILE";

		// Web request headers
		public const string WrhMoveFileTo = "MoveFileTo";

#if !KeePassLibSD
		// Allow self-signed certificates, expired certificates, etc.
		private static bool ValidateServerCertificate(object sender,
			X509Certificate certificate, X509Chain chain,
			SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		internal static void ConfigureWebRequest(WebRequest request)
		{
			if(request == null) { Debug.Assert(false); return; } // No throw

			// WebDAV support
			if(request is HttpWebRequest)
			{
				request.PreAuthenticate = true; // Also auth GET
				if(request.Method == WebRequestMethods.Http.Post)
					request.Method = WebRequestMethods.Http.Put;
			}
			// else if(request is FtpWebRequest)
			// {
			//	Debug.Assert(((FtpWebRequest)request).UsePassive);
			// }
		}

		private static void PrepareWebAccess()
		{
			ServicePointManager.ServerCertificateValidationCallback =
				ValidateServerCertificate;
		}

		private static IOWebClient CreateWebClient(IOConnectionInfo ioc)
		{
			PrepareWebAccess();

			IOWebClient wc = new IOWebClient();
			wc.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

			if((ioc.UserName.Length > 0) || (ioc.Password.Length > 0))
				wc.Credentials = new NetworkCredential(ioc.UserName, ioc.Password);

			return wc;
		}

		private static WebRequest CreateWebRequest(IOConnectionInfo ioc)
		{
			PrepareWebAccess();

			WebRequest req = WebRequest.Create(ioc.Path);
			ConfigureWebRequest(req);
			req.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

			if((ioc.UserName.Length > 0) || (ioc.Password.Length > 0))
				req.Credentials = new NetworkCredential(ioc.UserName, ioc.Password);

			return req;
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
			if(ioc == null) { Debug.Assert(false); return null; }

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

		public static bool FileExists(IOConnectionInfo ioc)
		{
			return FileExists(ioc, false);
		}

		public static bool FileExists(IOConnectionInfo ioc, bool bThrowErrors)
		{
			if(ioc == null) { Debug.Assert(false); return false; }

			if(ioc.IsLocalFile()) return File.Exists(ioc.Path);

			try
			{
				Stream s = OpenRead(ioc);
				if(s == null) throw new FileNotFoundException();

				// For FTP clients we called RETR to get the file, but we never
				// followed-up and downloaded the file. Close may produce a
				// 550 error -- that's okay
				try { s.Close(); }
				catch(Exception) { }
			}
			catch(Exception)
			{
				if(bThrowErrors) throw;
				return false;
			}

			return true;
		}

		public static void DeleteFile(IOConnectionInfo ioc)
		{
			if(ioc.IsLocalFile()) { File.Delete(ioc.Path); return; }

#if !KeePassLibSD
			WebRequest req = CreateWebRequest(ioc);
			if(req != null)
			{
				if(req is HttpWebRequest) req.Method = "DELETE";
				else if(req is FtpWebRequest) req.Method = WebRequestMethods.Ftp.DeleteFile;
				else if(req is FileWebRequest)
				{
					File.Delete(UrlUtil.FileUrlToPath(ioc.Path));
					return;
				}
				else req.Method = WrmDeleteFile;

				req.GetResponse();
			}
#endif
		}

		/// <summary>
		/// Rename/move a file. For local file system and WebDAV, the
		/// specified file is moved, i.e. the file destination can be
		/// in a different directory/path. In contrast, for FTP the
		/// file is renamed, i.e. its destination must be in the same
		/// directory/path.
		/// </summary>
		/// <param name="iocFrom">Source file path.</param>
		/// <param name="iocTo">Target file path.</param>
		public static void RenameFile(IOConnectionInfo iocFrom, IOConnectionInfo iocTo)
		{
			if(iocFrom.IsLocalFile()) { File.Move(iocFrom.Path, iocTo.Path); return; }

#if !KeePassLibSD
			WebRequest req = CreateWebRequest(iocFrom);
			if(req != null)
			{
				if(req is HttpWebRequest)
				{
					req.Method = "MOVE";
					req.Headers.Set("Destination", iocTo.Path); // Full URL supported
				}
				else if(req is FtpWebRequest)
				{
					req.Method = WebRequestMethods.Ftp.Rename;
					((FtpWebRequest)req).RenameTo = UrlUtil.GetFileName(iocTo.Path);
				}
				else if(req is FileWebRequest)
				{
					File.Move(UrlUtil.FileUrlToPath(iocFrom.Path),
						UrlUtil.FileUrlToPath(iocTo.Path));
					return;
				}
				else
				{
					req.Method = WrmMoveFile;
					req.Headers.Set(WrhMoveFileTo, iocTo.Path);
				}

				req.GetResponse();
			}
#endif

			// using(Stream sIn = IOConnection.OpenRead(iocFrom))
			// {
			//	using(Stream sOut = IOConnection.OpenWrite(iocTo))
			//	{
			//		MemUtil.CopyStream(sIn, sOut);
			//		sOut.Close();
			//	}
			//
			//	sIn.Close();
			// }
			// DeleteFile(iocFrom);
		}
	}
}
