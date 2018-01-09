/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2018 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.IO;

using KeePass.App;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography.KeyDerivation;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Serialization;

namespace KeePass.DataExchange.Formats
{
	internal class KeePassKdb2x : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return true; } }

		public override string FormatName { get { return "KeePass KDBX (2.x)"; } }
		public override string DefaultExtension { get { return AppDefs.FileExtension.FileExt; } }
		public override string ApplicationGroup { get { return PwDefs.ShortProductName; } }

		public override bool SupportsUuids { get { return true; } }
		public override bool RequiresKey { get { return true; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_KeePass; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			KdbxFile kdbx = new KdbxFile(pwStorage);
			kdbx.Load(sInput, KdbxFormat.Default, slLogger);
		}

		public override bool Export(PwExportInfo pwExportInfo, Stream sOutput,
			IStatusLogger slLogger)
		{
			KdbxFile kdbx = new KdbxFile(pwExportInfo.ContextDatabase);
			kdbx.Save(sOutput, pwExportInfo.DataGroup, KdbxFormat.Default, slLogger);
			return true;
		}
	}

	// KDBX 3.1 export module
	internal sealed class KeePassKdb2x3 : KeePassKdb2x
	{
		// Only list base class as import module
		public override bool SupportsImport { get { return false; } }

		public override string FormatName
		{
			get { return ("KeePass KDBX (2.34, " + KPRes.OldFormat + ")"); }
		}

		public override bool Export(PwExportInfo pwExportInfo, Stream sOutput,
			IStatusLogger slLogger)
		{
			PwDatabase pd = pwExportInfo.ContextDatabase;
			PwGroup pgRoot = pwExportInfo.DataGroup;

			// Remove everything that requires KDBX 4 or higher;
			// see also KdbxFile.GetMinKdbxVersion

			KdfParameters pKdf = pd.KdfParameters;
			AesKdf kdfAes = new AesKdf();
			if(!kdfAes.Uuid.Equals(pKdf.KdfUuid))
				pd.KdfParameters = kdfAes.GetDefaultParameters();

			VariantDictionary vdPublic = pd.PublicCustomData;
			pd.PublicCustomData = new VariantDictionary();

			List<PwGroup> lCustomGK = new List<PwGroup>();
			List<StringDictionaryEx> lCustomGV = new List<StringDictionaryEx>();
			List<PwEntry> lCustomEK = new List<PwEntry>();
			List<StringDictionaryEx> lCustomEV = new List<StringDictionaryEx>();

			GroupHandler gh = delegate(PwGroup pg)
			{
				if(pg == null) { Debug.Assert(false); return true; }
				if(pg.CustomData.Count > 0)
				{
					lCustomGK.Add(pg);
					lCustomGV.Add(pg.CustomData);
					pg.CustomData = new StringDictionaryEx();
				}
				return true;
			};
			EntryHandler eh = delegate(PwEntry pe)
			{
				if(pe == null) { Debug.Assert(false); return true; }
				if(pe.CustomData.Count > 0)
				{
					lCustomEK.Add(pe);
					lCustomEV.Add(pe.CustomData);
					pe.CustomData = new StringDictionaryEx();
				}
				return true;
			};

			gh(pgRoot);
			pgRoot.TraverseTree(TraversalMethod.PreOrder, gh, eh);

			try
			{
				KdbxFile kdbx = new KdbxFile(pd);
				kdbx.ForceVersion = KdbxFile.FileVersion32_3;
				kdbx.Save(sOutput, pgRoot, KdbxFormat.Default, slLogger);
			}
			finally
			{
				// Restore

				pd.KdfParameters = pKdf;
				pd.PublicCustomData = vdPublic;

				for(int i = 0; i < lCustomGK.Count; ++i)
					lCustomGK[i].CustomData = lCustomGV[i];
				for(int i = 0; i < lCustomEK.Count; ++i)
					lCustomEK[i].CustomData = lCustomEV[i];
			}

			return true;
		}
	}
}
