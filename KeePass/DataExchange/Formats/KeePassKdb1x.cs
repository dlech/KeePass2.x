using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;

namespace KeePass.DataExchange.Formats
{
	public sealed class KeePassKdb1x : FormatImporter
	{
		public override string FormatName { get { return "KeePass KDB 1.x"; } }
		public override string DefaultExtension { get { return "kdb"; } }
		public override string AppGroup { get { return PwDefs.ShortProductName; } }

		public override bool SupportsUuids { get { return true; } }
		public override bool RequiresKey { get { return true; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_KeePass; }
		}

		public override bool TryBeginImports()
		{
			if(!Kdb3File.IsLibraryInstalled())
			{
				string strKPLibC = KPRes.KeePassLibCNotFound + "\r\n\r\n";
				strKPLibC += KPRes.KDB3KeePassLibC;

				MessageBox.Show(strKPLibC, PwDefs.ShortProductName,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);

				return false;
			}

			return true;
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strTempFile = Path.GetTempFileName();
			try { File.Delete(strTempFile); }
			catch(Exception) { }

			BinaryReader br = new BinaryReader(sInput);
			byte[] pb = br.ReadBytes((int)sInput.Length);
			br.Close();
			File.WriteAllBytes(strTempFile, pb);

			Kdb3File kdb3 = new Kdb3File(pwStorage, slLogger);
			kdb3.Load(strTempFile);

			try { File.Delete(strTempFile); }
			catch(Exception) { }
		}
	}
}
