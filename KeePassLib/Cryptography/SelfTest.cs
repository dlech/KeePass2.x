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
using System.Diagnostics;
using System.Security;
using System.Security.Cryptography;

using KeePassLib.Cryptography.Cipher;
using KeePassLib.Keys;
using KeePassLib.Native;
using KeePassLib.Utility;
using KeePassLib.Resources;

namespace KeePassLib.Cryptography
{
/* #pragma warning disable 1591
	/// <summary>
	/// Return values of the <c>SelfTest.Perform</c> method.
	/// </summary>
	public enum SelfTestResult
	{
		Success = 0,
		RijndaelEcbError = 1,
		Salsa20Error = 2,
		NativeKeyTransformationError = 3
	}
#pragma warning restore 1591 */

	/// <summary>
	/// Class containing self-test methods.
	/// </summary>
	public static class SelfTest
	{
		/// <summary>
		/// Perform a self-test.
		/// </summary>
		public static void Perform()
		{
			TestFipsComplianceProblems(); // Must be the first test

			TestRijndael();
			TestSalsa20();
			TestNativeKeyTransform();
		}

		private static void TestFipsComplianceProblems()
		{
			try { new RijndaelManaged(); }
			catch(Exception exAes)
			{
				throw new SecurityException("AES/Rijndael: " + exAes.Message);
			}

			try { new SHA256Managed(); }
			catch(Exception exSha256)
			{
				throw new SecurityException("SHA-256: " + exSha256.Message);
			}
		}

		private static void TestRijndael()
		{
			// Test vector (official ECB test vector #356)
			byte[] pbIV = new byte[16];
			byte[] pbTestKey = new byte[32];
			byte[] pbTestData = new byte[16];
			byte[] pbReferenceCT = new byte[16] {
				0x75, 0xD1, 0x1B, 0x0E, 0x3A, 0x68, 0xC4, 0x22,
				0x3D, 0x88, 0xDB, 0xF0, 0x17, 0x97, 0x7D, 0xD7 };
			int i;

			for(i = 0; i < 16; ++i) pbIV[i] = 0;
			for(i = 0; i < 32; ++i) pbTestKey[i] = 0;
			for(i = 0; i < 16; ++i) pbTestData[i] = 0;
			pbTestData[0] = 0x04;

			RijndaelManaged r = new RijndaelManaged();

			if(r.BlockSize != 128) // AES block size
			{
				Debug.Assert(false);
				r.BlockSize = 128;
			}

			r.IV = pbIV;
			r.KeySize = 256;
			r.Key = pbTestKey;
			r.Mode = CipherMode.ECB;
			ICryptoTransform iCrypt = r.CreateEncryptor();

			iCrypt.TransformBlock(pbTestData, 0, 16, pbTestData, 0);

			if(!MemUtil.ArraysEqual(pbTestData, pbReferenceCT))
				throw new SecurityException(KLRes.EncAlgorithmAes + ".");
		}

		private static void TestSalsa20()
		{
			// Test values from official set 6, vector 3
			byte[] pbKey= new byte[32] {
				0x0F, 0x62, 0xB5, 0x08, 0x5B, 0xAE, 0x01, 0x54,
				0xA7, 0xFA, 0x4D, 0xA0, 0xF3, 0x46, 0x99, 0xEC,
				0x3F, 0x92, 0xE5, 0x38, 0x8B, 0xDE, 0x31, 0x84,
				0xD7, 0x2A, 0x7D, 0xD0, 0x23, 0x76, 0xC9, 0x1C
			};
			byte[] pbIV = new byte[8] { 0x28, 0x8F, 0xF6, 0x5D,
				0xC4, 0x2B, 0x92, 0xF9 };
			byte[] pbExpected = new byte[16] {
				0x5E, 0x5E, 0x71, 0xF9, 0x01, 0x99, 0x34, 0x03,
				0x04, 0xAB, 0xB2, 0x2A, 0x37, 0xB6, 0x62, 0x5B
			};

			byte[] pb = new byte[16];
			Salsa20Cipher c = new Salsa20Cipher(pbKey, pbIV);
			c.Encrypt(pb, pb.Length, false);
			if(!MemUtil.ArraysEqual(pb, pbExpected))
				throw new SecurityException("Salsa20.");

#if DEBUG
			// Extended test in debug mode
			byte[] pbExpected2 = new byte[16] {
				0xAB, 0xF3, 0x9A, 0x21, 0x0E, 0xEE, 0x89, 0x59,
				0x8B, 0x71, 0x33, 0x37, 0x70, 0x56, 0xC2, 0xFE
			};
			byte[] pbExpected3 = new byte[16] {
				0x1B, 0xA8, 0x9D, 0xBD, 0x3F, 0x98, 0x83, 0x97,
				0x28, 0xF5, 0x67, 0x91, 0xD5, 0xB7, 0xCE, 0x23
			};

			Random r = new Random();
			int nPos = Salsa20ToPos(c, r, pb.Length, 65536);
			c.Encrypt(pb, pb.Length, false);
			if(!MemUtil.ArraysEqual(pb, pbExpected2))
				throw new SecurityException("Salsa20-2.");

			nPos = Salsa20ToPos(c, r, nPos + pb.Length, 131008);
			Array.Clear(pb, 0, pb.Length);
			c.Encrypt(pb, pb.Length, true);
			if(!MemUtil.ArraysEqual(pb, pbExpected3))
				throw new SecurityException("Salsa20-3.");
#endif
		}

#if DEBUG
		private static int Salsa20ToPos(Salsa20Cipher c, Random r, int nPos,
			int nTargetPos)
		{
			byte[] pb = new byte[512];

			while(nPos < nTargetPos)
			{
				int x = r.Next(1, 513);
				int nGen = Math.Min(nTargetPos - nPos, x);
				c.Encrypt(pb, nGen, r.Next(0, 2) == 0);
				nPos += nGen;
			}

			return nTargetPos;
		}
#endif

		private static void TestNativeKeyTransform()
		{
#if DEBUG
			byte[] pbOrgKey = CryptoRandom.Instance.GetRandomBytes(32);
			byte[] pbSeed = CryptoRandom.Instance.GetRandomBytes(32);
			ulong uRounds = (ulong)((new Random()).Next(1, 0x3FFF));

			byte[] pbManaged = new byte[32];
			Array.Copy(pbOrgKey, pbManaged, 32);
			if(CompositeKey.TransformKeyManaged(pbManaged, pbSeed, uRounds) == false)
				throw new SecurityException("Managed transform.");

			byte[] pbNative = new byte[32];
			Array.Copy(pbOrgKey, pbNative, 32);
			if(NativeLib.TransformKey256(pbNative, pbSeed, uRounds) == false)
				return; // Native library not available ("success")

			if(!MemUtil.ArraysEqual(pbManaged, pbNative))
				throw new SecurityException("Native transform.");
#endif
		}
	}
}
