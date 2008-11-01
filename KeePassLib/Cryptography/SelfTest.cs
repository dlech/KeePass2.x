/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Security.Cryptography;

using KeePassLib.Utility;

namespace KeePassLib.Cryptography
{
#pragma warning disable 1591
	/// <summary>
	/// Return values of the <c>SelfTest.Perform</c> method.
	/// </summary>
	public enum SelfTestResult
	{
		Success = 0,
		RijndaelEcbError = 1
	}
#pragma warning restore 1591

	/// <summary>
	/// Class containing self-test methods.
	/// </summary>
	public static class SelfTest
	{
		/// <summary>
		/// Perform a self-test.
		/// </summary>
		/// <returns>A <c>SelfTestResult</c> error code.</returns>
		public static SelfTestResult Perform()
		{
			// Test vector (official ECB test vector #356)
			byte[] pbIV = new byte[16];
			byte[] pbTestKey = new byte[32];
			byte[] pbTestData = new byte[16];
			byte[] pbReferenceCT = new byte[16]{
				0x75, 0xD1, 0x1B, 0x0E, 0x3A, 0x68, 0xC4, 0x22,
				0x3D, 0x88, 0xDB, 0xF0, 0x17, 0x97, 0x7D, 0xD7 };
			uint i;

			for(i = 0; i < 16; ++i) pbIV[i] = 0;
			for(i = 0; i < 32; ++i) pbTestKey[i] = 0;
			for(i = 0; i < 16; ++i) pbTestData[i] = 0;
			pbTestData[0] = 0x04;

			RijndaelManaged r = new RijndaelManaged();
			r.IV = pbIV;
			r.KeySize = 256;
			r.Key = pbTestKey;
			r.Mode = CipherMode.ECB;
			ICryptoTransform iCrypt = r.CreateEncryptor();

			iCrypt.TransformBlock(pbTestData, 0, 16, pbTestData, 0);

			for(i = 0; i < 16; ++i)
			{
				Debug.Assert((i >= 0) && (i < 16));

				if(pbTestData[i] != pbReferenceCT[i])
				{
					Debug.Assert(false);
					return SelfTestResult.RijndaelEcbError;
				}
			}

			return SelfTestResult.Success;
		}
	}
}
