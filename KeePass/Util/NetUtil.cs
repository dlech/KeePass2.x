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
using System.Net;
using System.IO;
using System.IO.Compression;

namespace KeePass.Util
{
	public static class NetUtil
	{
		public static string GZipUtf8ResultToString(DownloadDataCompletedEventArgs e)
		{
			if(e.Cancelled || (e.Error != null) || (e.Result == null))
				return null;

			MemoryStream msZipped = new MemoryStream(e.Result);
			GZipStream gz = new GZipStream(msZipped, CompressionMode.Decompress);
			BinaryReader br = new BinaryReader(gz);
			MemoryStream msUTF8 = new MemoryStream();

			while(true)
			{
				byte[] pb = null;

				try { pb = br.ReadBytes(4096); }
				catch(Exception) { }

				if((pb == null) || (pb.Length == 0)) break;

				msUTF8.Write(pb, 0, pb.Length);
			}

			br.Close();
			gz.Close();
			msZipped.Close();

			return Encoding.UTF8.GetString(msUTF8.ToArray());
		}
	}
}
