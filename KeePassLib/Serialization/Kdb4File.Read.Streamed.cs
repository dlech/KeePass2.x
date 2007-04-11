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
		private enum KdbContext
		{
			Null = 0,
			KeePassFile,
			Meta,
			Root,
			MemoryProtection,
			CustomIcons,
			CustomIcon,
			RootDeletedObjects,
			DeletedObject,
			Group,
			GroupTimes,
			Entry,
			EntryTimes,
			EntryString,
			EntryBinary,
			EntryAutoType,
			EntryAutoTypeItem,
			EntryHistory
		}

		private bool m_bReadNextNode = true;
		private Stack<PwGroup> m_ctxGroups = new Stack<PwGroup>();
		private PwGroup m_ctxGroup = null;
		private PwEntry m_ctxEntry = null;
		private string m_ctxStringName = null;
		private ProtectedString m_ctxStringValue = null;
		private string m_ctxBinaryName = null;
		private ProtectedBinary m_ctxBinaryValue = null;
		private string m_ctxATName = null;
		private string m_ctxATSeq = null;
		private bool m_bEntryInHistory = false;
		private PwEntry m_ctxHistoryBase = null;
		private PwDeletedObject m_ctxDeletedObject = null;
		private PwUuid m_uuidCustomIconID = PwUuid.Zero;
		private byte[] m_pbCustomIconData = null;

		private void ReadXmlStreamed(Stream readerStream, Stream sParentStream)
		{
			ReadDocumentStreamed(CreateXmlReader(readerStream), sParentStream);
		}

		private static XmlReader CreateXmlReader(Stream readerStream)
		{
			XmlReaderSettings xmlSettings = new XmlReaderSettings();
			xmlSettings.CloseInput = true;
			xmlSettings.IgnoreComments = true;
			xmlSettings.IgnoreProcessingInstructions = true;
			xmlSettings.IgnoreWhitespace = true;

			return XmlReader.Create(readerStream, xmlSettings);
		}

		private void ReadDocumentStreamed(XmlReader xr, Stream sParentStream)
		{
			Debug.Assert(xr != null);
			if(xr == null) throw new ArgumentNullException("xr");

			m_ctxGroups.Clear();

			KdbContext ctx = KdbContext.Null;

			uint uTagCounter = 0;

			bool bSupportsStatus = (m_slLogger != null);
			try
			{
				sParentStream.Position.ToString();
				sParentStream.Length.ToString();
			}
			catch(Exception) { bSupportsStatus = false; }

			m_bReadNextNode = true;

			while(true)
			{
				if(m_bReadNextNode)
				{
					if(!xr.Read()) break;
				}
				else m_bReadNextNode = true;

				switch(xr.NodeType)
				{
					case XmlNodeType.Element:
						ctx = ReadXmlElement(ctx, xr);
						break;

					case XmlNodeType.EndElement:
						ctx = EndXmlElement(ctx, xr);
						break;

					case XmlNodeType.XmlDeclaration:
						break; // Ignore

					default:
						Debug.Assert(false);
						break;
				}

				++uTagCounter;
				if(((uTagCounter % 256) == 0) && bSupportsStatus)
					m_slLogger.SetProgress((uint)((sParentStream.Position * 100) /
						sParentStream.Length));
			}

			Debug.Assert(ctx == KdbContext.Null);
			if(ctx != KdbContext.Null) throw new FormatException();

			Debug.Assert(m_ctxGroups.Count == 0);
			if(m_ctxGroups.Count != 0) throw new FormatException();
		}

		private KdbContext ReadXmlElement(KdbContext ctx, XmlReader xr)
		{
			Debug.Assert(xr.NodeType == XmlNodeType.Element);

			switch(ctx)
			{
				case KdbContext.Null:
					if(xr.Name == ElemDocNode)
						return SwitchContext(ctx, KdbContext.KeePassFile, xr);
					else ReadUnknown(xr);
					break;

				case KdbContext.KeePassFile:
					if(xr.Name == ElemMeta)
						return SwitchContext(ctx, KdbContext.Meta, xr);
					else if(xr.Name == ElemRoot)
						return SwitchContext(ctx, KdbContext.Root, xr);
					else ReadUnknown(xr);
					break;

				case KdbContext.Meta:
					if(xr.Name == ElemGenerator)
						ReadString(xr); // Ignore
					else if(xr.Name == ElemDbName)
						m_pwDatabase.Name = ReadString(xr);
					else if(xr.Name == ElemDbDesc)
						m_pwDatabase.Description = ReadString(xr);
					else if(xr.Name == ElemDbDefaultUser)
						m_pwDatabase.DefaultUserName = ReadString(xr);
					else if(xr.Name == ElemDbMntncHistoryDays)
						m_pwDatabase.MaintenanceHistoryDays = ReadUInt(xr, 365);
					else if(xr.Name == ElemMemoryProt)
						return SwitchContext(ctx, KdbContext.MemoryProtection, xr);
					else if(xr.Name == ElemCustomIcons)
						return SwitchContext(ctx, KdbContext.CustomIcons, xr);
					else ReadUnknown(xr);
					break;

				case KdbContext.MemoryProtection:
					if(xr.Name == ElemProtTitle)
						m_pwDatabase.MemoryProtection.ProtectNotes = ReadBool(xr, false);
					else if(xr.Name == ElemProtUserName)
						m_pwDatabase.MemoryProtection.ProtectUserName = ReadBool(xr, false);
					else if(xr.Name == ElemProtPassword)
						m_pwDatabase.MemoryProtection.ProtectPassword = ReadBool(xr, true);
					else if(xr.Name == ElemProtURL)
						m_pwDatabase.MemoryProtection.ProtectUrl = ReadBool(xr, false);
					else if(xr.Name == ElemProtNotes)
						m_pwDatabase.MemoryProtection.ProtectNotes = ReadBool(xr, false);
					else if(xr.Name == ElemProtAutoHide)
						m_pwDatabase.MemoryProtection.AutoEnableVisualHiding = ReadBool(xr, true);
					else ReadUnknown(xr);
					break;

				case KdbContext.CustomIcons:
					if(xr.Name == ElemCustomIconItem)
						return SwitchContext(ctx, KdbContext.CustomIcon, xr);
					else ReadUnknown(xr);
					break;

				case KdbContext.CustomIcon:
					if(xr.Name == ElemCustomIconItemID)
						m_uuidCustomIconID = ReadUuid(xr);
					else if(xr.Name == ElemCustomIconItemData)
					{
						string strData = ReadString(xr);
						if((strData != null) && (strData.Length > 0))
							m_pbCustomIconData = Convert.FromBase64String(strData);
						else { Debug.Assert(false); }
					}
					else ReadUnknown(xr);
					break;

				case KdbContext.Root:
					if(xr.Name == ElemGroup)
					{
						Debug.Assert(m_ctxGroups.Count == 0);
						if(m_ctxGroups.Count != 0) throw new FormatException();

						m_pwDatabase.RootGroup = new PwGroup(false, false);
						m_ctxGroups.Push(m_pwDatabase.RootGroup);
						m_ctxGroup = m_ctxGroups.Peek();

						return SwitchContext(ctx, KdbContext.Group, xr);
					}
					else if(xr.Name == ElemDeletedObjects)
						return SwitchContext(ctx, KdbContext.RootDeletedObjects, xr);
					else ReadUnknown(xr);
					break;

				case KdbContext.Group:
					if(xr.Name == ElemUuid)
						m_ctxGroup.Uuid = ReadUuid(xr);
					else if(xr.Name == ElemName)
						m_ctxGroup.Name = ReadString(xr);
					else if(xr.Name == ElemIcon)
						m_ctxGroup.IconID = (PwIcon)ReadUInt(xr, (uint)PwIcon.Folder);
					else if(xr.Name == ElemCustomIconID)
						m_ctxGroup.CustomIconUuid = ReadUuid(xr);
					else if(xr.Name == ElemTimes)
						return SwitchContext(ctx, KdbContext.GroupTimes, xr);
					else if(xr.Name == ElemIsExpanded)
						m_ctxGroup.IsExpanded = ReadBool(xr, true);
					else if(xr.Name == ElemGroupDefaultAutoTypeSeq)
						m_ctxGroup.DefaultAutoTypeSequence = ReadString(xr);
					else if(xr.Name == ElemGroup)
					{
						m_ctxGroup = new PwGroup(false, false);

						m_ctxGroup.ParentGroup = m_ctxGroups.Peek();
						m_ctxGroup.ParentGroup.Groups.Add(m_ctxGroup);

						m_ctxGroups.Push(m_ctxGroup);

						return SwitchContext(ctx, KdbContext.Group, xr);
					}
					else if(xr.Name == ElemEntry)
					{
						m_ctxEntry = new PwEntry(m_ctxGroup, false, false);

						m_ctxEntry.ParentGroup = m_ctxGroup;
						m_ctxGroup.Entries.Add(m_ctxEntry);

						m_bEntryInHistory = false;
						return SwitchContext(ctx, KdbContext.Entry, xr);
					}
					else ReadUnknown(xr);
					break;

				case KdbContext.Entry:
					if(xr.Name == ElemUuid)
						m_ctxEntry.Uuid = ReadUuid(xr);
					else if(xr.Name == ElemIcon)
						m_ctxEntry.IconID = (PwIcon)ReadUInt(xr, (uint)PwIcon.Key);
					else if(xr.Name == ElemCustomIconID)
						m_ctxEntry.CustomIconUuid = ReadUuid(xr);
					else if(xr.Name == ElemFgColor)
					{
						string strColor = ReadString(xr);
						if((strColor != null) && (strColor.Length > 0))
							m_ctxEntry.ForegroundColor = ColorTranslator.FromHtml(strColor);
					}
					else if(xr.Name == ElemBgColor)
					{
						string strColor = ReadString(xr);
						if((strColor != null) && (strColor.Length > 0))
							m_ctxEntry.BackgroundColor = ColorTranslator.FromHtml(strColor);
					}
					else if(xr.Name == ElemOverrideUrl)
						m_ctxEntry.OverrideUrl = ReadString(xr);
					else if(xr.Name == ElemTimes)
						return SwitchContext(ctx, KdbContext.EntryTimes, xr);
					else if(xr.Name == ElemString)
						return SwitchContext(ctx, KdbContext.EntryString, xr);
					else if(xr.Name == ElemBinary)
						return SwitchContext(ctx, KdbContext.EntryBinary, xr);
					else if(xr.Name == ElemAutoType)
						return SwitchContext(ctx, KdbContext.EntryAutoType, xr);
					else if(xr.Name == ElemHistory)
					{
						Debug.Assert(m_bEntryInHistory == false);

						if(m_bEntryInHistory == false)
						{
							m_ctxHistoryBase = m_ctxEntry;
							return SwitchContext(ctx, KdbContext.EntryHistory, xr);
						}
						else ReadUnknown(xr);
					}
					else ReadUnknown(xr);
					break;

				case KdbContext.GroupTimes:
				case KdbContext.EntryTimes:
					ITimeLogger tl = ((ctx == KdbContext.GroupTimes) ?
						(ITimeLogger)m_ctxGroup : (ITimeLogger)m_ctxEntry);
					Debug.Assert(tl != null);

					if(xr.Name == ElemLastModTime)
						tl.LastModificationTime = ReadTime(xr);
					else if(xr.Name == ElemCreationTime)
						tl.CreationTime = ReadTime(xr);
					else if(xr.Name == ElemLastAccessTime)
						tl.LastAccessTime = ReadTime(xr);
					else if(xr.Name == ElemExpiryTime)
						tl.ExpiryTime = ReadTime(xr);
					else if(xr.Name == ElemExpires)
						tl.Expires = ReadBool(xr, false);
					else if(xr.Name == ElemUsageCount)
						tl.UsageCount = ReadULong(xr, 0);
					else ReadUnknown(xr);
					break;

				case KdbContext.EntryString:
					if(xr.Name == ElemKey)
						m_ctxStringName = ReadString(xr);
					else if(xr.Name == ElemValue)
						m_ctxStringValue = ReadProtectedString(xr);
					else ReadUnknown(xr);
					break;

				case KdbContext.EntryBinary:
					if(xr.Name == ElemKey)
						m_ctxBinaryName = ReadString(xr);
					else if(xr.Name == ElemValue)
						m_ctxBinaryValue = ReadProtectedBinary(xr);
					else ReadUnknown(xr);
					break;

				case KdbContext.EntryAutoType:
					if(xr.Name == ElemAutoTypeEnabled)
						m_ctxEntry.AutoType.Enabled = ReadBool(xr, true);
					else if(xr.Name == ElemAutoTypeObfuscation)
						m_ctxEntry.AutoType.ObfuscationOptions =
							(AutoTypeObfuscationOptions)ReadUInt(xr, 0);
					else if(xr.Name == ElemAutoTypeDefaultSeq)
						m_ctxEntry.AutoType.DefaultSequence = ReadString(xr);
					else if(xr.Name == ElemAutoTypeItem)
						return SwitchContext(ctx, KdbContext.EntryAutoTypeItem, xr);
					else ReadUnknown(xr);
					break;

				case KdbContext.EntryAutoTypeItem:
					if(xr.Name == ElemWindow)
						m_ctxATName = ReadString(xr);
					else if(xr.Name == ElemKeystrokeSequence)
						m_ctxATSeq = ReadString(xr);
					else ReadUnknown(xr);
					break;

				case KdbContext.EntryHistory:
					if(xr.Name == ElemEntry)
					{
						m_ctxEntry = new PwEntry(null, false, false);
						m_ctxHistoryBase.History.Add(m_ctxEntry);

						m_bEntryInHistory = true;
						return SwitchContext(ctx, KdbContext.Entry, xr);
					}
					else ReadUnknown(xr);
					break;

				case KdbContext.RootDeletedObjects:
					if(xr.Name == ElemDeletedObject)
					{
						m_ctxDeletedObject = new PwDeletedObject();
						m_pwDatabase.DeletedObjects.Add(m_ctxDeletedObject);

						return SwitchContext(ctx, KdbContext.DeletedObject, xr);
					}
					else ReadUnknown(xr);
					break;

				case KdbContext.DeletedObject:
					if(xr.Name == ElemUuid)
						m_ctxDeletedObject.Uuid = ReadUuid(xr);
					else if(xr.Name == ElemDeletionTime)
						m_ctxDeletedObject.DeletionTime = ReadTime(xr);
					else ReadUnknown(xr);
					break;

				default:
					ReadUnknown(xr);
					break;
			}

			return ctx;
		}

		private KdbContext EndXmlElement(KdbContext ctx, XmlReader xr)
		{
			Debug.Assert(xr.NodeType == XmlNodeType.EndElement);

			if((ctx == KdbContext.KeePassFile) && (xr.Name == ElemDocNode))
				return KdbContext.Null;
			else if((ctx == KdbContext.Meta) && (xr.Name == ElemMeta))
				return KdbContext.KeePassFile;
			else if((ctx == KdbContext.Root) && (xr.Name == ElemRoot))
				return KdbContext.KeePassFile;
			else if((ctx == KdbContext.MemoryProtection) && (xr.Name == ElemMemoryProt))
				return KdbContext.Meta;
			else if((ctx == KdbContext.CustomIcons) && (xr.Name == ElemCustomIcons))
				return KdbContext.Meta;
			else if((ctx == KdbContext.CustomIcon) && (xr.Name == ElemCustomIconItem))
			{
				if((m_uuidCustomIconID != PwUuid.Zero) && (m_pbCustomIconData != null))
				{
					m_pwDatabase.CustomIcons.Add(new PwCustomIcon(
						m_uuidCustomIconID, m_pbCustomIconData));

					m_uuidCustomIconID = PwUuid.Zero;
					m_pbCustomIconData = null;
				}
				else { Debug.Assert(false); }

				return KdbContext.CustomIcons;
			}
			else if((ctx == KdbContext.Group) && (xr.Name == ElemGroup))
			{
				if(PwUuid.Zero.EqualsValue(m_ctxGroup.Uuid))
				{
					Debug.Assert(false);
					m_ctxGroup.Uuid = new PwUuid(true);
				}

				m_ctxGroups.Pop();

				if(m_ctxGroups.Count == 0)
				{
					m_ctxGroup = null;
					return KdbContext.Root;
				}
				else
				{
					m_ctxGroup = m_ctxGroups.Peek();
					return KdbContext.Group;
				}
			}
			else if((ctx == KdbContext.GroupTimes) && (xr.Name == ElemTimes))
				return KdbContext.Group;
			else if((ctx == KdbContext.Entry) && (xr.Name == ElemEntry))
			{
				// Create new UUID if absent
				if(PwUuid.Zero.EqualsValue(m_ctxEntry.Uuid))
				{
					Debug.Assert(false);
					m_ctxEntry.Uuid = new PwUuid(true);
				}

				if(m_bEntryInHistory)
				{
					m_ctxEntry = m_ctxHistoryBase;
					return KdbContext.EntryHistory;
				}

				return KdbContext.Group;
			}
			else if((ctx == KdbContext.EntryTimes) && (xr.Name == ElemTimes))
				return KdbContext.Entry;
			else if((ctx == KdbContext.EntryString) && (xr.Name == ElemString))
			{
				m_ctxEntry.Strings.Set(m_ctxStringName, m_ctxStringValue);
				m_ctxStringName = null;
				m_ctxStringValue = null;
				return KdbContext.Entry;
			}
			else if((ctx == KdbContext.EntryBinary) && (xr.Name == ElemBinary))
			{
				m_ctxEntry.Binaries.Set(m_ctxBinaryName, m_ctxBinaryValue);
				m_ctxBinaryName = null;
				m_ctxBinaryValue = null;
				return KdbContext.Entry;
			}
			else if((ctx == KdbContext.EntryAutoType) && (xr.Name == ElemAutoType))
				return KdbContext.Entry;
			else if((ctx == KdbContext.EntryAutoTypeItem) && (xr.Name == ElemAutoTypeItem))
			{
				m_ctxEntry.AutoType.Set(m_ctxATName, m_ctxATSeq);
				m_ctxATName = null;
				m_ctxATSeq = null;
				return KdbContext.EntryAutoType;
			}
			else if((ctx == KdbContext.EntryHistory) && (xr.Name == ElemHistory))
			{
				m_bEntryInHistory = false;
				return KdbContext.Entry;
			}
			else if((ctx == KdbContext.RootDeletedObjects) && (xr.Name == ElemDeletedObjects))
				return KdbContext.Root;
			else if((ctx == KdbContext.DeletedObject) && (xr.Name == ElemDeletedObject))
			{
				m_ctxDeletedObject = null;
				return KdbContext.RootDeletedObjects;
			}
			else
			{
				Debug.Assert(false);
				throw new FormatException();
			}
		}

		private string ReadString(XmlReader xr)
		{
			XorredBuffer xb = ProcessNode(xr);

			if(xb != null)
			{
				byte[] pb = xb.ReadPlainText();
				return Encoding.UTF8.GetString(pb, 0, pb.Length);
			}

			m_bReadNextNode = false; // ReadElementString skips end tag
			return xr.ReadElementString();
		}

		private string ReadStringRaw(XmlReader xr)
		{
			m_bReadNextNode = false; // ReadElementString skips end tag
			return xr.ReadElementString();
		}

		private bool ReadBool(XmlReader xr, bool bDefault)
		{
			string str = ReadString(xr);
			if(str == ValTrue) return true;
			else if(str == ValFalse) return false;

			Debug.Assert(false);
			return bDefault;
		}

		private PwUuid ReadUuid(XmlReader xr)
		{
			string str = ReadString(xr);
			return new PwUuid(Convert.FromBase64String(str));
		}

		private uint ReadUInt(XmlReader xr, uint uDefault)
		{
			string str = ReadString(xr);

			uint u;
			if(StrUtil.TryParseUInt(str, out u)) return u;

			Debug.Assert(false);
			return uDefault;
		}

		private ulong ReadULong(XmlReader xr, ulong uDefault)
		{
			string str = ReadString(xr);

			ulong u;
			if(StrUtil.TryParseULong(str, out u)) return u;

			Debug.Assert(false);
			return uDefault;
		}

		private DateTime ReadTime(XmlReader xr)
		{
			string str = ReadString(xr);

			DateTime dt;
			if(StrUtil.TryParseDateTime(str, out dt)) return dt;

			Debug.Assert(false);
			return m_dtNow;
		}

		private ProtectedString ReadProtectedString(XmlReader xr)
		{
			XorredBuffer xb = ProcessNode(xr);

			if(xb != null) return new ProtectedString(true, xb);

			ProtectedString ps = new ProtectedString(false, ReadString(xr));
			return ps;
		}

		private ProtectedBinary ReadProtectedBinary(XmlReader xr)
		{
			XorredBuffer xb = ProcessNode(xr);

			if(xb != null) return new ProtectedBinary(true, xb);

			string strValue = ReadString(xr);
			if(strValue.Length == 0) return new ProtectedBinary(false);

			return new ProtectedBinary(false, Convert.FromBase64String(strValue));
		}

		private void ReadUnknown(XmlReader xr)
		{
			Debug.Assert(false); // Unknown node!

			if(xr.IsEmptyElement) return;

			string strUnknownName = xr.Name;
			ProcessNode(xr);

			while(xr.Read())
			{
				if(xr.NodeType == XmlNodeType.EndElement) break;
				if(xr.NodeType != XmlNodeType.Element) continue;

				ReadUnknown(xr);
			}

			Debug.Assert(xr.Name == strUnknownName);
		}

		private XorredBuffer ProcessNode(XmlReader xr)
		{
			Debug.Assert(xr.NodeType == XmlNodeType.Element);

			XorredBuffer xb = null;

			if(xr.HasAttributes)
			{
				if(xr.MoveToAttribute(AttrProtected))
				{
					if(xr.Value == ValTrue)
					{
						xr.MoveToElement();
						string strEncrypted = ReadStringRaw(xr);

						byte[] pbEncrypted;
						if(strEncrypted.Length > 0)
							pbEncrypted = Convert.FromBase64String(strEncrypted);
						else pbEncrypted = new byte[0];

						byte[] pbPad = m_randomStream.GetRandomBytes((uint)pbEncrypted.Length);

						xb = new XorredBuffer(pbEncrypted, pbPad);
					}
				}
			}

			return xb;
		}

		private static KdbContext SwitchContext(KdbContext ctxCurrent,
			KdbContext ctxNew, XmlReader xr)
		{
			if(xr.IsEmptyElement) return ctxCurrent;
			return ctxNew;
		}
	}
}
