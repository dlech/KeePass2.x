/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Ecas
{
	public sealed class EcasPool
	{
		private readonly List<EcasEventProvider> m_lEventProviders =
			new List<EcasEventProvider>();
		private readonly List<EcasConditionProvider> m_lConditionProviders =
			new List<EcasConditionProvider>();
		private readonly List<EcasActionProvider> m_lActionProviders =
			new List<EcasActionProvider>();

		internal List<EcasEventProvider> EventProviders
		{
			get { return m_lEventProviders; }
		}

		internal List<EcasConditionProvider> ConditionProviders
		{
			get { return m_lConditionProviders; }
		}

		internal List<EcasActionProvider> ActionProviders
		{
			get { return m_lActionProviders; }
		}

		public EcasPool()
		{
		}

		public EcasPool(bool bAddDefaultProviders)
		{
			if(bAddDefaultProviders) AddDefaultProviders();
		}

		private void AddDefaultProviders()
		{
			m_lEventProviders.Add(new EcasDefaultEventProvider());
			m_lConditionProviders.Add(new EcasDefaultConditionProvider());
			m_lActionProviders.Add(new EcasDefaultActionProvider());
		}

		public void AddEventProvider(EcasEventProvider p)
		{
			if(p == null) throw new ArgumentNullException("p");
			m_lEventProviders.Add(p);
		}

		public bool RemoveEventProvider(EcasEventProvider p)
		{
			if(p == null) throw new ArgumentNullException("p");
			return m_lEventProviders.Remove(p);
		}

		public EcasEventType FindEvent(string strEventName)
		{
			if(strEventName == null) throw new ArgumentNullException("strEventName");

			foreach(EcasEventProvider p in m_lEventProviders)
			{
				EcasEventType t = p.Find(strEventName);
				if(t != null) return t;
			}

			return null;
		}

		public EcasEventType FindEvent(PwUuid uuid)
		{
			if(uuid == null) throw new ArgumentNullException("uuid");

			foreach(EcasEventProvider p in m_lEventProviders)
			{
				EcasEventType t = p.Find(uuid);
				if(t != null) return t;
			}

			return null;
		}

		public void AddConditionProvider(EcasConditionProvider p)
		{
			if(p == null) throw new ArgumentNullException("p");
			m_lConditionProviders.Add(p);
		}

		public bool RemoveConditionProvider(EcasConditionProvider p)
		{
			if(p == null) throw new ArgumentNullException("p");
			return m_lConditionProviders.Remove(p);
		}

		public EcasConditionType FindCondition(string strConditionName)
		{
			if(strConditionName == null) throw new ArgumentNullException("strConditionName");

			foreach(EcasConditionProvider p in m_lConditionProviders)
			{
				EcasConditionType t = p.Find(strConditionName);
				if(t != null) return t;
			}

			return null;
		}

		public EcasConditionType FindCondition(PwUuid uuid)
		{
			if(uuid == null) throw new ArgumentNullException("uuid");

			foreach(EcasConditionProvider p in m_lConditionProviders)
			{
				EcasConditionType t = p.Find(uuid);
				if(t != null) return t;
			}

			return null;
		}

		public void AddActionProvider(EcasActionProvider p)
		{
			if(p == null) throw new ArgumentNullException("p");
			m_lActionProviders.Add(p);
		}

		public bool RemoveActionProvider(EcasActionProvider p)
		{
			if(p == null) throw new ArgumentNullException("p");
			return m_lActionProviders.Remove(p);
		}

		public EcasActionType FindAction(string strActionName)
		{
			if(strActionName == null) throw new ArgumentNullException("strActionName");

			foreach(EcasActionProvider p in m_lActionProviders)
			{
				EcasActionType t = p.Find(strActionName);
				if(t != null) return t;
			}

			return null;
		}

		public EcasActionType FindAction(PwUuid uuid)
		{
			if(uuid == null) throw new ArgumentNullException("uuid");

			foreach(EcasActionProvider p in m_lActionProviders)
			{
				EcasActionType t = p.Find(uuid);
				if(t != null) return t;
			}

			return null;
		}

		public bool CompareEvents(EcasEvent e, EcasContext ctx)
		{
			if(e == null) throw new ArgumentNullException("e");
			if(ctx == null) throw new ArgumentNullException("ctx");

			if(!e.Type.Equals(ctx.Event.Type)) return false;

			foreach(EcasEventProvider p in m_lEventProviders)
			{
				if(p.IsSupported(e.Type))
					return p.Compare(e, ctx);
			}

			throw new Exception(KPRes.TriggerEventTypeUnknown + " " +
				KPRes.TypeUnknownHint + MessageService.NewParagraph + e.TypeString);
		}

		public bool EvaluateCondition(EcasCondition c, EcasContext ctx)
		{
			if(c == null) throw new ArgumentNullException("c");

			foreach(EcasConditionProvider p in m_lConditionProviders)
			{
				if(p.IsSupported(c.Type))
				{
					bool bResult = p.Evaluate(c, ctx);
					return (c.Negate ? !bResult : bResult);
				}
			}

			throw new Exception(KPRes.TriggerConditionTypeUnknown + " " +
				KPRes.TypeUnknownHint + MessageService.NewParagraph + c.TypeString);
		}

		public void ExecuteAction(EcasAction a, EcasContext ctx)
		{
			if(a == null) throw new ArgumentNullException("a");

			foreach(EcasActionProvider p in m_lActionProviders)
			{
				if(p.IsSupported(a.Type))
				{
					p.Execute(a, ctx);
					return;
				}
			}

			throw new Exception(KPRes.TriggerActionTypeUnknown + " " +
				KPRes.TypeUnknownHint + MessageService.NewParagraph + a.TypeString);
		}
	}
}
