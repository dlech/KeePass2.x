/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2017 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Text;

#if !KeePassUAP
using System.Security.Cryptography;
#endif

using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePassLib.Cryptography.PasswordGenerator
{
	public enum PwgError
	{
		Success = 0,
		Unknown = 1,
		TooFewCharacters = 2,
		UnknownAlgorithm = 3
	}

	/// <summary>
	/// Utility functions for generating random passwords.
	/// </summary>
	public static class PwGenerator
	{
		public static PwgError Generate(out ProtectedString psOut,
			PwProfile pwProfile, byte[] pbUserEntropy,
			CustomPwGeneratorPool pwAlgorithmPool)
		{
			Debug.Assert(pwProfile != null);
			if(pwProfile == null) throw new ArgumentNullException("pwProfile");

			PwgError e = PwgError.Unknown;
			CryptoRandomStream crs = null;
			byte[] pbKey = null;
			try
			{
				crs = CreateRandomStream(pbUserEntropy, out pbKey);

				if(pwProfile.GeneratorType == PasswordGeneratorType.CharSet)
					e = CharSetBasedGenerator.Generate(out psOut, pwProfile, crs);
				else if(pwProfile.GeneratorType == PasswordGeneratorType.Pattern)
					e = PatternBasedGenerator.Generate(out psOut, pwProfile, crs);
				else if(pwProfile.GeneratorType == PasswordGeneratorType.Custom)
					e = GenerateCustom(out psOut, pwProfile, crs, pwAlgorithmPool);
				else { Debug.Assert(false); psOut = ProtectedString.Empty; }
			}
			finally
			{
				if(crs != null) crs.Dispose();
				if(pbKey != null) MemUtil.ZeroByteArray(pbKey);
			}

			return e;
		}

		private static CryptoRandomStream CreateRandomStream(byte[] pbAdditionalEntropy,
			out byte[] pbKey)
		{
			pbKey = CryptoRandom.Instance.GetRandomBytes(128);

			// Mix in additional entropy
			Debug.Assert(pbKey.Length >= 64);
			if((pbAdditionalEntropy != null) && (pbAdditionalEntropy.Length > 0))
			{
				using(SHA512Managed h = new SHA512Managed())
				{
					byte[] pbHash = h.ComputeHash(pbAdditionalEntropy);
					MemUtil.XorArray(pbHash, 0, pbKey, 0, pbHash.Length);
				}
			}

			return new CryptoRandomStream(CrsAlgorithm.ChaCha20, pbKey);
		}

		internal static char GenerateCharacter(PwProfile pwProfile,
			PwCharSet pwCharSet, CryptoRandomStream crsRandomSource)
		{
			if(pwCharSet.Size == 0) return char.MinValue;

			ulong uIndex = crsRandomSource.GetRandomUInt64();
			uIndex %= (ulong)pwCharSet.Size;

			char ch = pwCharSet[(uint)uIndex];

			if(pwProfile.NoRepeatingCharacters)
				pwCharSet.Remove(ch);

			return ch;
		}

		internal static void PrepareCharSet(PwCharSet pwCharSet, PwProfile pwProfile)
		{
			pwCharSet.Remove(PwCharSet.Invalid);

			if(pwProfile.ExcludeLookAlike) pwCharSet.Remove(PwCharSet.LookAlike);

			if(pwProfile.ExcludeCharacters.Length > 0)
				pwCharSet.Remove(pwProfile.ExcludeCharacters);
		}

		internal static void ShufflePassword(char[] pPassword,
			CryptoRandomStream crsRandomSource)
		{
			Debug.Assert(pPassword != null); if(pPassword == null) return;
			Debug.Assert(crsRandomSource != null); if(crsRandomSource == null) return;

			if(pPassword.Length <= 1) return; // Nothing to shuffle

			for(int nSelect = 0; nSelect < pPassword.Length; ++nSelect)
			{
				ulong uRandomIndex = crsRandomSource.GetRandomUInt64();
				uRandomIndex %= (ulong)(pPassword.Length - nSelect);

				char chTemp = pPassword[nSelect];
				pPassword[nSelect] = pPassword[nSelect + (int)uRandomIndex];
				pPassword[nSelect + (int)uRandomIndex] = chTemp;
			}
		}

		private static PwgError GenerateCustom(out ProtectedString psOut,
			PwProfile pwProfile, CryptoRandomStream crs,
			CustomPwGeneratorPool pwAlgorithmPool)
		{
			psOut = ProtectedString.Empty;

			Debug.Assert(pwProfile.GeneratorType == PasswordGeneratorType.Custom);
			if(pwAlgorithmPool == null) return PwgError.UnknownAlgorithm;

			string strID = pwProfile.CustomAlgorithmUuid;
			if(string.IsNullOrEmpty(strID)) { Debug.Assert(false); return PwgError.UnknownAlgorithm; }

			byte[] pbUuid = Convert.FromBase64String(strID);
			PwUuid uuid = new PwUuid(pbUuid);
			CustomPwGenerator pwg = pwAlgorithmPool.Find(uuid);
			if(pwg == null) { Debug.Assert(false); return PwgError.UnknownAlgorithm; }

			ProtectedString pwd = pwg.Generate(pwProfile.CloneDeep(), crs);
			if(pwd == null) return PwgError.Unknown;

			psOut = pwd;
			return PwgError.Success;
		}
	}
}
