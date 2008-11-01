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
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.Drawing;
using System.Xml;
using System.IO;
using System.Diagnostics;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePassLib.Serialization
{
	public sealed partial class Kdb4File
	{
		/*
		private void ReadXmlDom(Stream readerStream)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(readerStream);

			XmlElement el = doc.DocumentElement;
			ReadDocument(el);
		}

		private void ReadDocument(XmlNode xmlRootNode)
		{
			Debug.Assert(xmlRootNode != null);
			if(xmlRootNode == null) throw new ArgumentNullException("xmlRootNode");
			if(xmlRootNode.Name != ElemDocNode) throw new XmlException("xmlRootNode");

			foreach(XmlNode xmlChild in xmlRootNode.ChildNodes)
			{
				string strNodeName = xmlChild.Name;

				if(strNodeName == ElemMeta) ReadMeta(xmlChild);
				else if(strNodeName == ElemRoot) ReadRoot(xmlChild);
				else ReadUnknown(xmlChild);
			}
		}
		*/

		private void ReadUnknown(XmlNode xmlNode)
		{
			ProcessNode(xmlNode);

			// if(m_slLogger != null)
			//	m_slLogger.SetText("Unknown field: " + xmlNode.Name, LogStatusType.Warning);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
				ReadUnknown(xmlChild);
		}

		private XorredBuffer ProcessNode(XmlNode xmlNode)
		{
			Debug.Assert(xmlNode != null);
			if(xmlNode == null) throw new ArgumentNullException("xmlNode");

			XmlAttributeCollection xac = xmlNode.Attributes;
			if(xac == null) return null;

			XmlNode xmlProtected = xac.GetNamedItem(AttrProtected);
			if(xmlProtected != null)
			{
				if(xmlProtected.Value == ValTrue)
				{
					string strInner = xmlNode.InnerText;

					byte[] pbEncrypted;
					if(strInner.Length > 0)
						pbEncrypted = Convert.FromBase64String(strInner);
					else pbEncrypted = new byte[0];

					byte[] pbPad = m_randomStream.GetRandomBytes((uint)pbEncrypted.Length);

					return new XorredBuffer(pbEncrypted, pbPad);
				}
			}

			return null;
		}

		/*
		private void ReadMeta(XmlNode xmlNode)
		{
			ProcessNode(xmlNode);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				string strName = xmlChild.Name;

				if(strName == ElemGenerator) { } // Ignore
				else if(strName == ElemDbName)
					m_pwDatabase.Name = ReadString(xmlChild);
				else if(strName == ElemDbDesc)
					m_pwDatabase.Description = ReadString(xmlChild);
				else if(strName == ElemDbDefaultUser)
					m_pwDatabase.DefaultUserName = ReadString(xmlChild);
				else if(strName == ElemDbMntncHistoryDays)
					m_pwDatabase.MaintenanceHistoryDays = ReadUInt(xmlChild, 365);
				else if(strName == ElemMemoryProt)
					ReadMemoryProtection(xmlChild);
				else if(strName == ElemCustomIcons)
					ReadCustomIcons(xmlChild);
				else if(strName == ElemLastSelectedGroup)
					m_pwDatabase.LastSelectedGroup = ReadUuid(xmlChild);
				else if(strName == ElemLastTopVisibleGroup)
					m_pwDatabase.LastTopVisibleGroup = ReadUuid(xmlChild);
				else ReadUnknown(xmlChild);
			}
		}

		private void ReadMemoryProtection(XmlNode xmlNode)
		{
			ProcessNode(xmlNode);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				string strName = xmlChild.Name;

				if(strName == ElemProtTitle)
					m_pwDatabase.MemoryProtection.ProtectTitle = ReadBool(xmlChild, false);
				else if(strName == ElemProtUserName)
					m_pwDatabase.MemoryProtection.ProtectUserName = ReadBool(xmlChild, false);
				else if(strName == ElemProtPassword)
					m_pwDatabase.MemoryProtection.ProtectPassword = ReadBool(xmlChild, true);
				else if(strName == ElemProtURL)
					m_pwDatabase.MemoryProtection.ProtectUrl = ReadBool(xmlChild, false);
				else if(strName == ElemProtNotes)
					m_pwDatabase.MemoryProtection.ProtectNotes = ReadBool(xmlChild, false);
				else if(strName == ElemProtAutoHide)
					m_pwDatabase.MemoryProtection.AutoEnableVisualHiding = ReadBool(xmlChild, true);
				else ReadUnknown(xmlChild);
			}
		}

		private void ReadCustomIcons(XmlNode xmlNode)
		{
			ProcessNode(xmlNode);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				string strName = xmlChild.Name;

				if(strName == ElemCustomIconItem)
					ReadCustomIcon(xmlChild);
				else ReadUnknown(xmlChild);
			}
		}

		private void ReadCustomIcon(XmlNode xmlNode)
		{
			ProcessNode(xmlNode);

			PwUuid uuid = PwUuid.Zero;
			byte[] pbImageData = null;

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				string strName = xmlChild.Name;

				if(strName == ElemCustomIconItemID)
					uuid = ReadUuid(xmlChild);
				else if(strName == ElemCustomIconItemData)
				{
					string str = ReadString(xmlChild);

					if((str != null) && (str.Length > 0))
						pbImageData = Convert.FromBase64String(str);
					else { Debug.Assert(false); }
				}
				else ReadUnknown(xmlChild);
			}

			if((uuid != PwUuid.Zero) && (pbImageData != null))
				m_pwDatabase.CustomIcons.Add(new PwCustomIcon(uuid, pbImageData));
			else { Debug.Assert(false); }
		}

		private void ReadRoot(XmlNode xmlNode)
		{
			ProcessNode(xmlNode);

			bool bFoundDataGroup = false;
			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				string strName = xmlChild.Name;

				if(strName == ElemGroup)
				{
					if(!bFoundDataGroup)
					{
						m_pwDatabase.RootGroup = ReadGroup(xmlChild);
						bFoundDataGroup = true;
					}
					else { Debug.Assert(false); ReadUnknown(xmlChild); }
				}
				else if(strName == ElemDeletedObjects)
				{
					ReadDeletedObjects(xmlChild, m_pwDatabase.DeletedObjects);
				}
				else ReadUnknown(xmlChild);
			}

			Debug.Assert(m_pwDatabase.RootGroup != null);
			Debug.Assert(m_pwDatabase.RootGroup.ParentGroup == null);
		}

		private PwGroup ReadGroup(XmlNode xmlNode)
		{
			ProcessNode(xmlNode);

			PwGroup pgStorage = new PwGroup(false, false);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				string strName = xmlChild.Name;

				if(strName == ElemUuid) pgStorage.Uuid = ReadUuid(xmlChild);
				else if(strName == ElemName) pgStorage.Name = ReadString(xmlChild);
				else if(strName == ElemNotes) pgStorage.Notes = ReadString(xmlChild);
				else if(strName == ElemIcon) pgStorage.IconID = (PwIcon)ReadUInt(xmlChild, (uint)PwIcon.Key);
				else if(strName == ElemCustomIconID) pgStorage.CustomIconUuid = ReadUuid(xmlChild);
				else if(strName == ElemTimes) ReadTimes(xmlChild, pgStorage);
				else if(strName == ElemIsExpanded)
					pgStorage.IsExpanded = ReadBool(xmlChild, true);
				else if(strName == ElemGroupDefaultAutoTypeSeq)
					pgStorage.DefaultAutoTypeSequence = ReadString(xmlChild);
				else if(strName == ElemLastTopVisibleEntry)
					pgStorage.LastTopVisibleEntry = ReadUuid(xmlChild);
				else if(strName == ElemGroup)
				{
					PwGroup pgSub = ReadGroup(xmlChild);
					pgSub.ParentGroup = pgStorage;
					pgStorage.Groups.Add(pgSub);
				}
				else if(strName == ElemEntry)
				{
					PwEntry pe = ReadEntry(xmlChild);
					pgStorage.AddEntry(pe, true);
				}
				else ReadUnknown(xmlChild);
			}

			// Create new UUID if absent
			if(PwUuid.Zero.EqualsValue(pgStorage.Uuid))
				pgStorage.Uuid = new PwUuid(true);

			return pgStorage;
		}
		*/

		private PwEntry ReadEntry(XmlNode xmlNode)
		{
			ProcessNode(xmlNode);

			PwEntry pe = new PwEntry(false, false);

			Debug.Assert(Color.Empty.ToArgb() == 0);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				string strName = xmlChild.Name;

				if(strName == ElemUuid) pe.Uuid = ReadUuid(xmlChild);
				else if(strName == ElemIcon)
					pe.IconId = (PwIcon)ReadUInt(xmlChild, (uint)PwIcon.Key);
				else if(strName == ElemCustomIconID)
					pe.CustomIconUuid = ReadUuid(xmlChild);
				else if(strName == ElemFgColor)
				{
					string strColor = ReadString(xmlChild);
					if((strColor != null) && (strColor.Length > 0))
						pe.ForegroundColor = ColorTranslator.FromHtml(strColor);
				}
				else if(strName == ElemBgColor)
				{
					string strColor = ReadString(xmlChild);
					if((strColor != null) && (strColor.Length > 0))
						pe.BackgroundColor = ColorTranslator.FromHtml(strColor);
				}
				else if(strName == ElemOverrideUrl)
					pe.OverrideUrl = ReadString(xmlChild);
				else if(strName == ElemTimes) ReadTimes(xmlChild, pe);
				else if(strName == ElemString) ReadProtectedStringEx(xmlChild, pe.Strings);
				else if(strName == ElemBinary) ReadProtectedBinaryEx(xmlChild, pe.Binaries);
				else if(strName == ElemAutoType) ReadAutoType(xmlChild, pe.AutoType);
				else if(strName == ElemHistory) ReadHistory(xmlChild, pe.History);
				else ReadUnknown(xmlChild);
			}

			// Create new UUID if absent
			if(PwUuid.Zero.EqualsValue(pe.Uuid))
				pe.Uuid = new PwUuid(true);

			return pe;
		}

		private void ReadTimes(XmlNode xmlNode, ITimeLogger times)
		{
			ProcessNode(xmlNode);

			foreach(XmlNode xmlChild in xmlNode)
			{
				string strName = xmlChild.Name;

				if(strName == ElemLastModTime)
					times.LastModificationTime = ReadTime(xmlChild);
				else if(strName == ElemCreationTime)
					times.CreationTime = ReadTime(xmlChild);
				else if(strName == ElemLastAccessTime)
					times.LastAccessTime = ReadTime(xmlChild);
				else if(strName == ElemExpiryTime)
					times.ExpiryTime = ReadTime(xmlChild);
				else if(strName == ElemExpires)
					times.Expires = ReadBool(xmlChild, false);
				else if(strName == ElemUsageCount)
					times.UsageCount = ReadULong(xmlChild, 0);
				else
					ReadUnknown(xmlChild);
			}
		}

		private string ReadString(XmlNode xmlNode)
		{
			ProcessNode(xmlNode);

			return xmlNode.InnerText;
		}

		private bool ReadBool(XmlNode xmlNode, bool bDefault)
		{
			ProcessNode(xmlNode);

			string str = xmlNode.InnerText;
			if(str == ValTrue) return true;
			else if(str == ValFalse) return false;

			Debug.Assert(false);
			return bDefault;
		}

		private PwUuid ReadUuid(XmlNode xmlNode)
		{
			ProcessNode(xmlNode);

			return new PwUuid(Convert.FromBase64String(xmlNode.InnerText));
		}

		private uint ReadUInt(XmlNode xmlNode, uint uDefault)
		{
			ProcessNode(xmlNode);

			uint u;
			if(StrUtil.TryParseUInt(xmlNode.InnerText, out u)) return u;

			Debug.Assert(false);
			return uDefault;
		}

		private ulong ReadULong(XmlNode xmlNode, ulong uDefault)
		{
			ProcessNode(xmlNode);

			ulong u;
			if(StrUtil.TryParseULong(xmlNode.InnerText, out u)) return u;

			Debug.Assert(false);
			return uDefault;
		}

		private DateTime ReadTime(XmlNode xmlNode)
		{
			ProcessNode(xmlNode);

			DateTime dt;
			if(StrUtil.TryParseDateTime(xmlNode.InnerText, out dt)) return dt;

			Debug.Assert(false);
			return m_dtNow;
		}

		private void ReadProtectedStringEx(XmlNode xmlNode, ProtectedStringDictionary dictStorage)
		{
			ProcessNode(xmlNode);

			string strKey = string.Empty;
			XorredBuffer xbValue = null;
			string strValue = null;

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name == ElemKey)
				{
					ProcessNode(xmlChild);
					strKey = xmlChild.InnerText;
				}
				else if(xmlChild.Name == ElemValue)
				{
					xbValue = ProcessNode(xmlChild);

					// If contents aren't protected: read as plain-text string
					if(xbValue == null) strValue = xmlChild.InnerText;
				}
				else ReadUnknown(xmlChild);
			}

			if(xbValue != null)
			{
				Debug.Assert(strValue == null);
				dictStorage.Set(strKey, new ProtectedString(true, xbValue));
			}
			else
			{
				Debug.Assert(strValue != null);
				dictStorage.Set(strKey, new ProtectedString(false, strValue));
			}

#if DEBUG
			if(m_format == Kdb4Format.Default)
			{
				if(strKey == PwDefs.TitleField)
				{
					Debug.Assert(m_pwDatabase.MemoryProtection.ProtectTitle ==
						dictStorage.Get(strKey).IsProtected);
				}
				else if(strKey == PwDefs.UserNameField)
				{
					Debug.Assert(m_pwDatabase.MemoryProtection.ProtectUserName ==
						dictStorage.Get(strKey).IsProtected);
				}
				else if(strKey == PwDefs.PasswordField)
				{
					Debug.Assert(m_pwDatabase.MemoryProtection.ProtectPassword ==
						dictStorage.Get(strKey).IsProtected);
				}
				else if(strKey == PwDefs.UrlField)
				{
					Debug.Assert(m_pwDatabase.MemoryProtection.ProtectUrl ==
						dictStorage.Get(strKey).IsProtected);
				}
				else if(strKey == PwDefs.NotesField)
				{
					Debug.Assert(m_pwDatabase.MemoryProtection.ProtectNotes ==
						dictStorage.Get(strKey).IsProtected);
				}
			}
#endif
		}

		private void ReadProtectedBinaryEx(XmlNode xmlNode, ProtectedBinaryDictionary dictStorage)
		{
			ProcessNode(xmlNode);

			string strKey = string.Empty;
			XorredBuffer xbValue = null;
			byte[] pbValue = null;

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name == ElemKey)
				{
					ProcessNode(xmlChild);
					strKey = xmlChild.InnerText;
				}
				else if(xmlChild.Name == ElemValue)
				{
					xbValue = ProcessNode(xmlChild);

					if(xbValue == null)
					{
						string strInner = xmlChild.InnerText;

						if(strInner.Length > 0)
							pbValue = Convert.FromBase64String(strInner);
						else pbValue = new byte[0];
					}
				}
				else ReadUnknown(xmlChild);
			}

			if(xbValue != null)
			{
				Debug.Assert(pbValue == null);
				dictStorage.Set(strKey, new ProtectedBinary(true, xbValue));
			}
			else
			{
				Debug.Assert(pbValue != null);
				dictStorage.Set(strKey, new ProtectedBinary(false, pbValue));
			}
		}

		private void ReadAutoType(XmlNode xmlNode, AutoTypeConfig atConfig)
		{
			ProcessNode(xmlNode);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name == ElemAutoTypeEnabled)
					atConfig.Enabled = ReadBool(xmlChild, true);
				else if(xmlChild.Name == ElemAutoTypeObfuscation)
					atConfig.ObfuscationOptions =
						(AutoTypeObfuscationOptions)ReadUInt(xmlChild, 0);
				else if(xmlChild.Name == ElemAutoTypeDefaultSeq)
					atConfig.DefaultSequence = ReadString(xmlChild);
				else if(xmlChild.Name == ElemAutoTypeItem)
					ReadAutoTypeItem(xmlChild, atConfig);
				else ReadUnknown(xmlChild);
			}
		}

		private void ReadAutoTypeItem(XmlNode xmlNode, AutoTypeConfig atStorage)
		{
			ProcessNode(xmlNode);

			string strWindow = string.Empty, strKeySeq = string.Empty;

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name == ElemWindow)
					strWindow = ReadString(xmlChild);
				else if(xmlChild.Name == ElemKeystrokeSequence)
					strKeySeq = ReadString(xmlChild);
				else ReadUnknown(xmlChild);
			}

			atStorage.Set(strWindow, strKeySeq);
		}

		/*
		private void ReadDeletedObjects(XmlNode xmlNode, PwObjectList<PwDeletedObject> list)
		{
			ProcessNode(xmlNode);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name == ElemDeletedObject)
					list.Add(ReadDeletedObject(xmlChild));
				else ReadUnknown(xmlChild);
			}
		}

		private PwDeletedObject ReadDeletedObject(XmlNode xmlNode)
		{
			ProcessNode(xmlNode);

			PwDeletedObject pdo = new PwDeletedObject();
			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name == ElemUuid)
					pdo.Uuid = ReadUuid(xmlChild);
				else if(xmlChild.Name == ElemDeletionTime)
					pdo.DeletionTime = ReadTime(xmlChild);
				else ReadUnknown(xmlChild);
			}

			return pdo;
		}
		*/

		private void ReadHistory(XmlNode xmlNode, PwObjectList<PwEntry> plStorage)
		{
			ProcessNode(xmlNode);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name == ElemEntry)
				{
					plStorage.Add(ReadEntry(xmlChild));
				}
				else ReadUnknown(xmlChild);
			}
		}
	}
}
