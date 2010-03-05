/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.App;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Utility;
using KeePassLib.Serialization;

namespace KeePass.Util
{
	public static class KeyUtil
	{
		public static CompositeKey KeyFromCommandLine(CommandLineArgs args)
		{
			if(args == null) throw new ArgumentNullException("args");

			CompositeKey cmpKey = new CompositeKey();
			string strPassword = args[AppDefs.CommandLineOptions.Password];
			string strPasswordEnc = args[AppDefs.CommandLineOptions.PasswordEncrypted];
			string strKeyFile = args[AppDefs.CommandLineOptions.KeyFile];
			string strUserAcc = args[AppDefs.CommandLineOptions.UserAccount];

			if(strPassword != null)
				cmpKey.AddUserKey(new KcpPassword(strPassword));
			else if(strPasswordEnc != null)
				cmpKey.AddUserKey(new KcpPassword(StrUtil.DecryptString(strPasswordEnc)));
			
			if(strKeyFile != null)
			{
				if(Program.KeyProviderPool.IsKeyProvider(strKeyFile))
				{
					KeyProviderQueryContext ctxKP = new KeyProviderQueryContext(
						IOConnectionInfo.FromPath(args.FileName), false);

					bool bPerformHash;
					byte[] pbProvKey = Program.KeyProviderPool.GetKey(strKeyFile, ctxKP,
						out bPerformHash);
					if((pbProvKey != null) && (pbProvKey.Length > 0))
					{
						try { cmpKey.AddUserKey(new KcpCustomKey(strKeyFile, pbProvKey, bPerformHash)); }
						catch(Exception exCKP)
						{
							MessageService.ShowWarning(exCKP);
							return null;
						}

						Array.Clear(pbProvKey, 0, pbProvKey.Length);
					}
					else return null; // Provider has shown error message
				}
				else // Key file
				{
					try { cmpKey.AddUserKey(new KcpKeyFile(strKeyFile)); }
					catch(Exception exKey)
					{
						MessageService.ShowWarning(strKeyFile, KPRes.KeyFileError, exKey);
						return null;
					}
				}
			}
			
			if(strUserAcc != null)
			{
				try { cmpKey.AddUserKey(new KcpUserAccount()); }
				catch(Exception exUA)
				{
					MessageService.ShowWarning(exUA);
					return null;
				}
			}

			return ((cmpKey.UserKeyCount > 0) ? cmpKey : null);
		}
	}
}
