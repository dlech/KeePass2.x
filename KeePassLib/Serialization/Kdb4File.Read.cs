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
using System.Diagnostics;
using System.Security;
using System.Security.Cryptography;
using System.Xml;

#if !KeePassLibSD
using System.IO.Compression;
#else
using KeePassLibSD;
#endif

using KeePassLib.Cryptography;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Interfaces;
using KeePassLib.Keys;
using KeePassLib.Resources;
using KeePassLib.Utility;

namespace KeePassLib.Serialization
{
	/// <summary>
	/// Serialization to KeePass KDB files.
	/// </summary>
	public sealed partial class Kdb4File
	{
		/// <summary>
		/// Load a KDB file from a file.
		/// </summary>
		/// <param name="strFilePath">File to load.</param>
		/// <param name="kdbFormat">Format specifier.</param>
		/// <param name="slLogger">Status logger (optional).</param>
		public void Load(string strFilePath, Kdb4Format kdbFormat, IStatusLogger slLogger)
		{
			IOConnectionInfo ioc = IOConnectionInfo.FromPath(strFilePath);
			this.Load(IOConnection.OpenRead(ioc), kdbFormat, slLogger);
		}

		/// <summary>
		/// Load a KDB file from a stream.
		/// </summary>
		/// <param name="sSource">Stream to read the data from. Must contain
		/// a KDB4 stream.</param>
		/// <param name="kdbFormat">Format specifier.</param>
		/// <param name="slLogger">Status logger (optional).</param>
		public void Load(Stream sSource, Kdb4Format kdbFormat, IStatusLogger slLogger)
		{
			Debug.Assert(sSource != null);
			if(sSource == null) throw new ArgumentNullException("sSource");

			m_format = kdbFormat;
			m_slLogger = slLogger;

			UTF8Encoding encNoBom = new UTF8Encoding(false, false);

			try
			{
				BinaryReader br = null;
				BinaryReader brDecrypted = null;
				Stream readerStream = null;

				if(kdbFormat == Kdb4Format.Default)
				{
					br = new BinaryReader(sSource, encNoBom);
					ReadHeader(br);

					Stream sDecrypted = AttachStreamDecryptor(sSource);
					if((sDecrypted == null) || (sDecrypted == sSource))
						throw new SecurityException(KLRes.CryptoStreamFailed);

					brDecrypted = new BinaryReader(sDecrypted, encNoBom);
					byte[] pbStoredStartBytes = brDecrypted.ReadBytes(32);

					if((pbStoredStartBytes == null) || (pbStoredStartBytes.Length != 32) ||
						(m_pbStreamStartBytes == null) || (m_pbStreamStartBytes.Length != 32))
					{
						throw new InvalidDataException();
					}

					for(int iStart = 0; iStart < 32; ++iStart)
					{
						if(pbStoredStartBytes[iStart] != m_pbStreamStartBytes[iStart])
							throw new InvalidCompositeKeyException(null);
					}

					Stream sHashed = new HashedBlockStream(sDecrypted, false);

					if(m_pwDatabase.Compression == PwCompressionAlgorithm.GZip)
						readerStream = new GZipStream(sHashed, CompressionMode.Decompress);
					else readerStream = sHashed;
				}
				else if(kdbFormat == Kdb4Format.PlainXml)
					readerStream = sSource;
				else { Debug.Assert(false); throw new FormatException("KdbFormat"); }

				if(kdbFormat != Kdb4Format.PlainXml) // Is an encrypted format
				{
					if(m_pbProtectedStreamKey == null)
					{
						Debug.Assert(false);
						throw new SecurityException("Invalid protected stream key!");
					}

					m_randomStream = new CryptoRandomStream(CrsAlgorithm.ArcFour, m_pbProtectedStreamKey);
				}
				else m_randomStream = null; // No random stream for plain text files

				ReadXmlStreamed(readerStream, sSource);
				// ReadXmlDom(readerStream);

				GC.KeepAlive(brDecrypted);
				GC.KeepAlive(br);
				
				sSource.Close();
			}
			catch(Exception)
			{
				sSource.Close();
				throw;
			}
		}

		private void ReadHeader(BinaryReader br)
		{
			Debug.Assert(br != null);
			if(br == null) throw new ArgumentNullException("br");

			byte[] pbSig1 = br.ReadBytes(4);
			uint uSig1 = MemUtil.BytesToUInt32(pbSig1);
			byte[] pbSig2 = br.ReadBytes(4);
			uint uSig2 = MemUtil.BytesToUInt32(pbSig2);

			if((uSig1 == FileSignatureOld1) && (uSig2 == FileSignatureOld2))
				throw new OldFormatException(PwDefs.ShortProductName + @" 1.x");

			if((uSig1 != FileSignature1) || (uSig2 != FileSignature2))
				throw new FormatException(KLRes.FileSigInvalid);

			byte[] pb = br.ReadBytes(4);
			uint uVersion = MemUtil.BytesToUInt32(pb);
			if((uVersion > FileVersion32) && (m_slLogger != null))
				m_slLogger.SetText(KLRes.FileVersionUnknown, LogStatusType.Warning);

			while(true)
			{
				if(ReadHeaderField(br) == false)
					break;
			}
		}

		private bool ReadHeaderField(BinaryReader brSource)
		{
			Debug.Assert(brSource != null);
			if(brSource == null) throw new ArgumentNullException("brSource");

			byte btFieldID = brSource.ReadByte();
			ushort uSize = MemUtil.BytesToUInt16(brSource.ReadBytes(2));

			byte[] pbData = null;
			if(uSize > 0)
			{
				pbData = brSource.ReadBytes(uSize);
				if((pbData == null) || (pbData.Length != uSize))
					throw new EndOfStreamException(KLRes.FileHeaderEndEarly);
			}

			bool bResult = true;
			Kdb4HeaderFieldID kdbID = (Kdb4HeaderFieldID)btFieldID;
			switch(kdbID)
			{
				case Kdb4HeaderFieldID.EndOfHeader:
					bResult = false; // Returning false indicates end of header
					break;

				case Kdb4HeaderFieldID.CipherID:
					SetCipher(pbData);
					break;

				case Kdb4HeaderFieldID.CompressionFlags:
					SetCompressionFlags(pbData);
					break;

				case Kdb4HeaderFieldID.MasterSeed:
					m_pbMasterSeed = pbData;
					break;

				case Kdb4HeaderFieldID.TransformSeed:
					m_pbTransformSeed = pbData;
					break;

				case Kdb4HeaderFieldID.TransformRounds:
					m_pwDatabase.KeyEncryptionRounds = MemUtil.BytesToUInt64(pbData);
					break;

				case Kdb4HeaderFieldID.EncryptionIV:
					m_pbEncryptionIV = pbData;
					break;

				case Kdb4HeaderFieldID.ProtectedStreamKey:
					m_pbProtectedStreamKey = pbData;
					break;

				case Kdb4HeaderFieldID.StreamStartBytes:
					m_pbStreamStartBytes = pbData;
					break;

				default:
					Debug.Assert(false);
					if(m_slLogger != null)
						m_slLogger.SetText(KLRes.UnknownHeaderID + @": " +
							kdbID.ToString() + "!", LogStatusType.Warning);
					break;
			}

			return bResult;
		}

		private void SetCipher(byte[] pbID)
		{
			if((pbID == null) || (pbID.Length != 16))
				throw new FormatException(KLRes.FileUnknownCipher);

			m_pwDatabase.DataCipherUuid = new PwUuid(pbID);
		}

		private void SetCompressionFlags(byte[] pbFlags)
		{
			uint uID = MemUtil.BytesToUInt32(pbFlags);
			if(uID >= (uint)PwCompressionAlgorithm.Count)
				throw new FormatException(KLRes.FileUnknownCompression);

			m_pwDatabase.Compression = (PwCompressionAlgorithm)uID;
		}

		private Stream AttachStreamDecryptor(Stream s)
		{
			MemoryStream ms = new MemoryStream();

			Debug.Assert(m_pbMasterSeed.Length == 32);
			if(m_pbMasterSeed.Length != 32)
				throw new FormatException(KLRes.MasterSeedLengthInvalid);
			ms.Write(m_pbMasterSeed, 0, 32);

			byte[] pKey32 = m_pwDatabase.MasterKey.GenerateKey32(m_pbTransformSeed,
				m_pwDatabase.KeyEncryptionRounds).ReadData();
			if((pKey32 == null) || (pKey32.Length != 32))
				throw new SecurityException(KLRes.InvalidCompositeKey);
			ms.Write(pKey32, 0, 32);
			
			SHA256Managed sha256 = new SHA256Managed();
			byte[] aesKey = sha256.ComputeHash(ms.ToArray());

			ms.Close();
			Array.Clear(pKey32, 0, 32);

			if((aesKey == null) || (aesKey.Length != 32))
				throw new SecurityException(KLRes.FinalKeyCreationFailed);

			ICipherEngine iEngine = CipherPool.GlobalPool.GetCipher(m_pwDatabase.DataCipherUuid);
			if(iEngine == null) throw new SecurityException(KLRes.FileUnknownCipher);
			return iEngine.DecryptStream(s, aesKey, m_pbEncryptionIV);
		}

		/// <summary>
		/// Read entries from a stream.
		/// </summary>
		/// <param name="pwDatabase">Source database.</param>
		/// <param name="msData">Input stream to read the entries from.</param>
		/// <returns>Extracted entries.</returns>
		public static List<PwEntry> ReadEntries(PwDatabase pwDatabase, Stream msData)
		{
			Kdb4File f = new Kdb4File(pwDatabase);
			f.m_format = Kdb4Format.PlainXml;

			XmlDocument doc = new XmlDocument();
			doc.Load(msData);

			XmlElement el = doc.DocumentElement;
			if(el.Name != ElemRoot) throw new FormatException();

			List<PwEntry> vEntries = new List<PwEntry>();

			foreach(XmlNode xmlChild in el.ChildNodes)
			{
				if(xmlChild.Name == ElemEntry)
				{
					PwEntry pe = f.ReadEntry(xmlChild);
					pe.Uuid = new PwUuid(true);

					foreach(PwEntry peHistory in pe.History)
						peHistory.Uuid = pe.Uuid;

					vEntries.Add(pe);
				}
				else { Debug.Assert(false); }
			}

			return vEntries;
		}
	}
}
