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
using KeePassLib.Utility;

namespace KeePassLib.Serialization
{
	/// <summary>
	/// Serialization to KeePass KDB files.
	/// </summary>
	public sealed partial class Kdb4File
	{
		public FileOpenResult Load(string strFile, KdbFormat kdbFormat, IStatusLogger slLogger)
		{
			IOConnectionInfo ioc = new IOConnectionInfo();
			Stream sIn;
			FileOpenResult fr = IOConnection.OpenRead(ioc, out sIn);
			if(fr.Code == FileOpenResultCode.Success)
				return this.Load(sIn, kdbFormat, slLogger);

			return fr;
		}

		/// <summary>
		/// Load a KDB file.
		/// </summary>
		/// <param name="sSource">Stream to read the data from. Must contain
		/// a KDB4 stream.</param>
		/// <param name="kdbFormat">Format specifier.</param>
		/// <param name="slLogger">Status logger (optional).</param>
		/// <returns>Error code.</returns>
		public FileOpenResult Load(Stream sSource, KdbFormat kdbFormat, IStatusLogger slLogger)
		{
			Debug.Assert(sSource != null);
			if(sSource == null) throw new ArgumentNullException("sSource");

			m_format = kdbFormat;
			m_slLogger = slLogger;

			BinaryReader br = null;
			Stream readerStream = null;

			if(kdbFormat == KdbFormat.Default)
			{
				try
				{
					br = new BinaryReader(sSource);
					ReadHeader(br);
				}
				catch(Exception excpHeader)
				{
					return new FileOpenResult(FileOpenResultCode.InvalidHeader, excpHeader);
				}

				Stream sDecrypted = null;
				try { sDecrypted = AttachStreamDecryptor(sSource); }
				catch(Exception secExcp)
				{
					return new FileOpenResult(FileOpenResultCode.SecurityException, secExcp);
				}
				if((sDecrypted == null) || (sDecrypted == sSource))
					return new FileOpenResult(FileOpenResultCode.SecurityException, null);

				try
				{
					if(m_pwDatabase.Compression == PwCompressionAlgorithm.GZip)
						readerStream = new GZipStream(sDecrypted, CompressionMode.Decompress);
					else readerStream = sDecrypted;
				}
				catch(Exception cprEx)
				{
					return new FileOpenResult(FileOpenResultCode.UnknownError, cprEx);
				}
			}
			else if(kdbFormat == KdbFormat.PlainXml)
				readerStream = sSource;
			else { Debug.Assert(false); throw new ArgumentException("Unknown KDB format!"); }

			if(kdbFormat != KdbFormat.PlainXml) // Is an encrypted format
			{
				if(m_pbProtectedStreamKey == null)
				{
					Debug.Assert(false);
					return new FileOpenResult(FileOpenResultCode.InvalidHeader,
						new Exception("No stream key present!"));
				}

				try
				{
					m_randomStream = new CryptoRandomStream(CrsAlgorithm.ArcFour, m_pbProtectedStreamKey);
				}
				catch(Exception)
				{
					return new FileOpenResult(FileOpenResultCode.SecurityException,
						new Exception("Unable to initialize CryptoRandomStream!"));
				}
			}
			else m_randomStream = null;

			FileOpenResult fr = ReadXmlStreamed(readerStream, sSource);
			// FileOpenResult fr = ReadXmlDom(readerStream);

			GC.KeepAlive(br);
			sSource.Close();
			return fr;
		}

		private void ReadHeader(BinaryReader br)
		{
			Debug.Assert(br != null); if(br == null) throw new ArgumentNullException("br");

			byte[] pb = br.ReadBytes(4);
			if(MemUtil.BytesToUInt32(pb) != FileSignature1)
				throw new FormatException("File signature invalid!");
			pb = br.ReadBytes(4);
			if(MemUtil.BytesToUInt32(pb) != FileSignature2)
				throw new FormatException("File signature invalid!");

			pb = br.ReadBytes(4);
			uint uVersion = MemUtil.BytesToUInt32(pb);
			if((uVersion > FileVersion32) && (m_slLogger != null))
				m_slLogger.SetText("The file format is newer than the currently supported one. Data loss is possible.", LogStatusType.Warning);

			while(true)
			{
				if(ReadHeaderField(br) == false)
					break;
			}
		}

		private bool ReadHeaderField(BinaryReader brSource)
		{
			Debug.Assert(brSource != null); if(brSource == null) throw new ArgumentNullException("brSource");

			byte btFieldID = brSource.ReadByte();
			ushort uSize = MemUtil.BytesToUInt16(brSource.ReadBytes(2));

			byte[] pbData = null;
			if(uSize > 0)
			{
				pbData = brSource.ReadBytes(uSize);
				if((pbData == null) || (pbData.Length != uSize))
					throw new EndOfStreamException("Header corrupted! Header data declared but not present!");
			}

			// Unknown field ID?
			if(btFieldID >= (byte)Kdb4HeaderFieldID.KnownFieldCount)
			{
				// if(m_slLogger != null)
				//	m_slLogger.SetText("Unknown header field ID: " + btFieldID.ToString() + "!", LogStatusType.Warning);
				Debug.Assert(false);
				return true;
			}

			bool bResult = true;
			Kdb4HeaderFieldID kdbID = (Kdb4HeaderFieldID)btFieldID;
			switch(kdbID)
			{
				case Kdb4HeaderFieldID.EndOfHeader:
					bResult = false;
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

				default:
					// if(m_slLogger != null)
					//	m_slLogger.SetText("Unknown header field ID: " + kdbID.ToString() + "!", LogStatusType.Warning);
					break;
			}

			return bResult;
		}

		private void SetCipher(byte[] pbID)
		{
			if((pbID == null) || (pbID.Length != 16))
				throw new SecurityException("The file has been encrypted using an unknown encryption algorithm.");

			m_pwDatabase.DataCipherUuid = new PwUuid(pbID);
		}

		private void SetCompressionFlags(byte[] pbFlags)
		{
			uint uID = MemUtil.BytesToUInt32(pbFlags);
			if(uID >= (uint)PwCompressionAlgorithm.Count)
				throw new SecurityException("The file has been compressed using an unknown compression algorithm.");

			m_pwDatabase.Compression = (PwCompressionAlgorithm)uID;
		}

		private Stream AttachStreamDecryptor(Stream s)
		{
			MemoryStream ms = new MemoryStream();

			Debug.Assert(m_pbMasterSeed.Length == 32);
			if(m_pbMasterSeed.Length != 32)
				throw new SecurityException("Master seed isn't of correct length!");
			ms.Write(m_pbMasterSeed, 0, 32);

			byte[] pKey32 = m_pwDatabase.MasterKey.GenerateKey32(m_pbTransformSeed,
				m_pwDatabase.KeyEncryptionRounds).ReadData();
			if((pKey32 == null) || (pKey32.Length != 32))
				throw new SecurityException("Invalid composite key!");
			ms.Write(pKey32, 0, 32);
			
			SHA256Managed sha256 = new SHA256Managed();
			byte[] aesKey = sha256.ComputeHash(ms.ToArray());

			ms.Close();
			Array.Clear(pKey32, 0, 32);

			if(aesKey == null) throw new SecurityException("Failed to create final decryption AES key!");
			if(aesKey.Length != 32) throw new SecurityException("Final AES decryption key isn't of correct length!");

			ICipherEngine iEngine = CipherPool.GlobalPool.GetCipher(m_pwDatabase.DataCipherUuid);
			if(iEngine == null) throw new SecurityException("The file has been encrypted using an unknown encryption algorithm.");
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
			f.m_format = KdbFormat.PlainXml;

			XmlDocument doc = new XmlDocument();
			try { doc.Load(msData); }
			catch(Exception) { Debug.Assert(false); return null; }

			XmlElement el = doc.DocumentElement;
			if(el.Name != ElemRoot) { Debug.Assert(false); return null; }

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
