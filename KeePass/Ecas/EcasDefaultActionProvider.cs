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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

using KeePass.Resources;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.Ecas
{
	internal sealed class EcasDefaultActionProvider : EcasActionProvider
	{
		private const uint IdTriggerOff = 0;
		private const uint IdTriggerOn = 1;
		private const uint IdTriggerToggle = 2;

		public EcasDefaultActionProvider()
		{
			m_actions.Add(new EcasActionType(new PwUuid(new byte[] {
				0xDA, 0xE5, 0xF8, 0x3B, 0x07, 0x30, 0x4C, 0x13,
				0x9E, 0xEF, 0x2E, 0xBA, 0xCB, 0x6E, 0xE4, 0xC7 }),
				KPRes.ExecuteCmdLineUrl, PwIcon.Console, new EcasParameter[] {
					new EcasParameter(KPRes.FileOrUrl, EcasValueType.String, null),
					new EcasParameter(KPRes.Arguments, EcasValueType.String, null) },
				ExecuteShellCmd));

			m_actions.Add(new EcasActionType(new PwUuid(new byte[] {
				0xB6, 0x46, 0xA6, 0x9F, 0xDE, 0x94, 0x4B, 0xB9,
				0x9B, 0xAE, 0x3C, 0xA4, 0x7E, 0xCC, 0x10, 0xEA }),
				KPRes.TriggerStateChange, PwIcon.Run, new EcasParameter[] {
					new EcasParameter(KPRes.TriggerName, EcasValueType.String, null),
					new EcasParameter(KPRes.NewState, EcasValueType.EnumStrings,
						new EcasEnum(new EcasEnumItem[] {
							new EcasEnumItem(IdTriggerOn, KPRes.On),
							new EcasEnumItem(IdTriggerOff, KPRes.Off),
							new EcasEnumItem(IdTriggerToggle, KPRes.Toggle) })) },
				ChangeTriggerOnOff));

			m_actions.Add(new EcasActionType(new PwUuid(new byte[] {
				0xFD, 0x41, 0x55, 0xD5, 0x79, 0x8F, 0x44, 0xFA,
				0xAB, 0x89, 0xF2, 0xF8, 0x70, 0xEF, 0x94, 0xB8 }),
				KPRes.OpenDatabaseFileStc, PwIcon.FolderOpen, new EcasParameter[] {
					new EcasParameter(KPRes.FileOrUrl, EcasValueType.String, null) },
				OpenDatabaseFile));

			m_actions.Add(new EcasActionType(new PwUuid(new byte[] {
				0x3B, 0x3D, 0x3E, 0x31, 0xE4, 0xB3, 0x42, 0xA6,
				0xBA, 0xCC, 0xD5, 0xC0, 0x3B, 0xAC, 0xA9, 0x69 }),
				KPRes.Wait, PwIcon.Clock, new EcasParameter[] {
					new EcasParameter(KPRes.TimeSpan + @" [ms]", EcasValueType.UInt64, null) },
				ExecuteSleep));

			m_actions.Add(new EcasActionType(new PwUuid(new byte[] {
				0x40, 0x69, 0xA5, 0x36, 0x57, 0x1B, 0x47, 0x92,
				0xA9, 0xB3, 0x73, 0x65, 0x30, 0xE0, 0xCF, 0xC3 }),
				KPRes.PerformGlobalAutoType, PwIcon.Run, null, ExecuteGlobalAutoType));

			m_actions.Add(new EcasActionType(new PwUuid(new byte[] {
				0x95, 0x81, 0x8F, 0x45, 0x99, 0x66, 0x49, 0x88,
				0xAB, 0x3E, 0x86, 0xE8, 0x1A, 0x96, 0x68, 0x36 }),
				KPRes.AddCustomToolBarButton, PwIcon.List, new EcasParameter[] {
					new EcasParameter(KPRes.Id, EcasValueType.String, null),
					new EcasParameter(KPRes.Name, EcasValueType.String, null),
					new EcasParameter(KPRes.Description, EcasValueType.String, null) },
				AddToolBarButton));
		}

		private static void ExecuteShellCmd(EcasAction a, EcasContext ctx)
		{
			string strCmd = EcasUtil.GetParamString(a.Parameters, 0);
			string strArgs = EcasUtil.GetParamString(a.Parameters, 1);

			if(string.IsNullOrEmpty(strCmd)) return;

			string strCmdEx = SprEngine.Compile(strCmd, false, null,
				Program.MainForm.ActiveDatabase, false, false);
			if(string.IsNullOrEmpty(strCmdEx)) return;

			string strArgsEx = ((strArgs != null) ? SprEngine.Compile(strArgs,
				false, null, Program.MainForm.ActiveDatabase, false, false) : null);

			try
			{
				if(string.IsNullOrEmpty(strArgsEx)) Process.Start(strCmdEx);
				else Process.Start(strCmdEx, strArgsEx);
			}
			catch(Exception e)
			{
				throw new Exception(strCmdEx + MessageService.NewParagraph + e.Message);
			}
		}

		private static void ChangeTriggerOnOff(EcasAction a, EcasContext ctx)
		{
			string strName = EcasUtil.GetParamString(a.Parameters, 0);
			uint uState = EcasUtil.GetParamUInt(a.Parameters, 1);

			EcasTrigger t = null;
			if(strName.Length == 0) t = ctx.Trigger;
			else
			{
				foreach(EcasTrigger trg in ctx.TriggerSystem.TriggerCollection)
				{
					if(trg.Name == strName) { t = trg; break; }
				}
			}

			if(t == null) throw new Exception(KPRes.ObjectNotFound +
				MessageService.NewParagraph + KPRes.TriggerName + ": " + strName + ".");

			if(uState == IdTriggerOn) t.On = true;
			else if(uState == IdTriggerOff) t.On = false;
			else if(uState == IdTriggerToggle) t.On = !t.On;
			else { Debug.Assert(false); }
		}

		private static void OpenDatabaseFile(EcasAction a, EcasContext ctx)
		{
			string strPath = EcasUtil.GetParamString(a.Parameters, 0);
			if(string.IsNullOrEmpty(strPath)) return;

			IOConnectionInfo ioc = IOConnectionInfo.FromPath(strPath);

			Program.MainForm.OpenDatabase(ioc, null, true);
		}

		private static void ExecuteSleep(EcasAction a, EcasContext ctx)
		{
			uint uTimeSpan = EcasUtil.GetParamUInt(a.Parameters, 0);

			if((uTimeSpan != 0) && (uTimeSpan <= (uint)int.MaxValue))
				Thread.Sleep((int)uTimeSpan);
		}

		private static void ExecuteGlobalAutoType(EcasAction a, EcasContext ctx)
		{
			Program.MainForm.ExecuteGlobalAutoType();
		}

		private static void AddToolBarButton(EcasAction a, EcasContext ctx)
		{
			string strID = EcasUtil.GetParamString(a.Parameters, 0);
			string strName = EcasUtil.GetParamString(a.Parameters, 1);
			string strDesc = EcasUtil.GetParamString(a.Parameters, 2);

			Program.MainForm.AddCustomToolBarButton(strID, strName, strDesc);
		}
	}
}
