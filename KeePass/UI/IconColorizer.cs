/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2020 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using KeePass.Native;

using KeePassLib.Utility;

using ICONDIR = KeePass.Native.NativeMethods.ICONDIR;
using ICONDIRENTRY = KeePass.Native.NativeMethods.ICONDIRENTRY;
using BITMAPINFOHEADER = KeePass.Native.NativeMethods.BITMAPINFOHEADER;

namespace KeePass.UI
{
	// This code has been written specifically for recoloring KeePass icons;
	// other icons may be unsupported.
	internal static class IconColorizer
	{
		private sealed class IcImage
		{
			public ICONDIRENTRY Entry;
			public byte[] Data;
		}

		public static Icon Recolor(Icon ico, Color clr)
		{
			if(ico == null) { Debug.Assert(false); return null; }

			try
			{
				float fHue, fSaturation, fValue;
				UIUtil.ColorToHsv(clr, out fHue, out fSaturation, out fValue);

				List<IcImage> l = GetImages(ico);

				for(int i = l.Count - 1; i >= 0; --i)
				{
					if(!RecolorImage(l[i], fHue)) l.RemoveAt(i);
				}

				Icon icoNew = CreateIcon(l);
				if(icoNew != null) return icoNew;
			}
			catch(Exception) { Debug.Assert(false); }

			return (ico.Clone() as Icon);
		}

		private static List<IcImage> GetImages(Icon ico)
		{
			List<IcImage> l = new List<IcImage>();

			byte[] pb;
			using(MemoryStream ms = new MemoryStream())
			{
				ico.Save(ms);
				pb = ms.ToArray();
			}

			const int cbDir = NativeMethods.ICONDIRSize;
			if(Marshal.SizeOf(typeof(ICONDIR)) != cbDir) { Debug.Assert(false); return l; }
			const int cbEntry = NativeMethods.ICONDIRENTRYSize;
			if(Marshal.SizeOf(typeof(ICONDIRENTRY)) != cbEntry) { Debug.Assert(false); return l; }

			int iPos = 0;

			ICONDIR d = MemUtil.BytesToStruct<ICONDIR>(pb, iPos);
			iPos += cbDir;

			if(d.idReserved != 0) { Debug.Assert(false); return l; }
			if(d.idType != 1) { Debug.Assert(false); return l; }

			for(uint u = 0; u < d.idCount; ++u)
			{
				IcImage img = new IcImage();

				img.Entry = MemUtil.BytesToStruct<ICONDIRENTRY>(pb, iPos);
				iPos += cbEntry;

				img.Data = MemUtil.Mid(pb, (int)img.Entry.dwImageOffset,
					(int)img.Entry.dwBytesInRes);

				l.Add(img);
			}

#if DEBUG
			int cbSum = cbDir + (l.Count * cbEntry);
			foreach(IcImage img in l) cbSum += img.Data.Length;
			Debug.Assert(cbSum == pb.Length); // No extra data
#endif

			return l;
		}

		private static Icon CreateIcon(List<IcImage> l)
		{
			int n = l.Count;
			if((n <= 0) || (n > ushort.MaxValue)) { Debug.Assert(false); return null; }

			const int cbDir = NativeMethods.ICONDIRSize;
			if(Marshal.SizeOf(typeof(ICONDIR)) != cbDir) { Debug.Assert(false); return null; }
			const int cbEntry = NativeMethods.ICONDIRENTRYSize;
			if(Marshal.SizeOf(typeof(ICONDIRENTRY)) != cbEntry) { Debug.Assert(false); return null; }

			int iData = cbDir + (n * cbEntry);
			for(int i = 0; i < n; ++i)
			{
				int cb = l[i].Data.Length;

				Debug.Assert(l[i].Entry.dwBytesInRes == cb);
				l[i].Entry.dwBytesInRes = (uint)cb;
				l[i].Entry.dwImageOffset = (uint)iData;
				iData += cb;
			}

			byte[] pbIco;
			using(MemoryStream ms = new MemoryStream(iData))
			{
				ICONDIR d = new ICONDIR();
				d.idType = 1;
				d.idCount = (ushort)n;

				byte[] pb = MemUtil.StructToBytes<ICONDIR>(ref d);
				Debug.Assert(pb.Length == cbDir);
				MemUtil.Write(ms, pb);

				for(int i = 0; i < n; ++i)
				{
					ICONDIRENTRY e = l[i].Entry;
					pb = MemUtil.StructToBytes<ICONDIRENTRY>(ref e);
					Debug.Assert(pb.Length == cbEntry);
					MemUtil.Write(ms, pb);
				}

				for(int i = 0; i < n; ++i)
					MemUtil.Write(ms, l[i].Data);

				pbIco = ms.ToArray();
			}
			Debug.Assert(pbIco.Length == iData);

			Icon ico;
			using(MemoryStream msRead = new MemoryStream(pbIco, false))
			{
				ico = new Icon(msRead);
			}
			return ico;
		}

		private static bool RecolorImage(IcImage img, float fHue)
		{
			const int cbHeader = NativeMethods.BITMAPINFOHEADERSize;
			if(Marshal.SizeOf(typeof(BITMAPINFOHEADER)) != cbHeader)
			{
				Debug.Assert(false);
				return false;
			}

			byte[] pb = img.Data;
			if(pb.Length < cbHeader) { Debug.Assert(false); return false; }

			BITMAPINFOHEADER h = MemUtil.BytesToStruct<BITMAPINFOHEADER>(pb, 0);
			if(h.biSize != cbHeader)
			{
				Debug.Assert(h.biSize == 0x474E5089); // PNG
				return false;
			}
			Debug.Assert(h.biPlanes == 1);
			Debug.Assert((h.biClrUsed == 0) && (h.biClrImportant == 0));
			if(h.biCompression != 0) { Debug.Assert(false); return false; }

			int w = h.biWidth;
			if(w <= 0) { Debug.Assert(false); return false; }
			int hAbs = Math.Abs(h.biHeight);
			if((hAbs != w) && (hAbs != (w << 1))) { Debug.Assert(false); return false; }

			ushort uBpp = h.biBitCount;
			if((uBpp == 1) || (uBpp == 4) || (uBpp == 8))
				RecolorPalette(pb, cbHeader, 1 << uBpp, fHue);
			else if((uBpp == 24) || (uBpp == 32))
			{
				if(!RecolorPixelData(pb, cbHeader, w, w, uBpp, fHue)) return false;
			}
			else { Debug.Assert(false); return false; }

			Debug.Assert(((w < 256) && (img.Entry.bWidth == w)) ||
				((w >= 256) && (img.Entry.bWidth == 0)));
			Debug.Assert(((w < 256) && (img.Entry.bHeight == w)) ||
				((w >= 256) && (img.Entry.bHeight == 0)));
			Debug.Assert(((uBpp < 8) && (img.Entry.bColorCount == (1 << uBpp))) ||
				((uBpp >= 8) && (img.Entry.bColorCount == 0)));
			Debug.Assert(img.Entry.bReserved == 0);
			Debug.Assert(img.Entry.wPlanes == h.biPlanes);
			Debug.Assert(img.Entry.wBitCount == uBpp);

			return true;
		}

		private static void RecolorPalette(byte[] pb, int iOffset,
			int cColors, float fHue)
		{
			int iMax = iOffset + (cColors * 4);
			for(int i = iOffset; i < iMax; i += 4)
			{
				RecolorColorBgr(pb, i, fHue);
				Debug.Assert(pb[i + 3] == 0); // rgbReserved of RGBQUAD must be 0
			}
		}

		private static bool RecolorPixelData(byte[] pb, int iOffset,
			int w, int h, ushort uBpp, float fHue)
		{
			int cbPixel;
			if(uBpp == 24)
			{
				// The code below does not support row padding to a
				// multiple of 4
				if((w & 3) != 0) { Debug.Assert(false); return false; }

				cbPixel = 3;
			}
			else if(uBpp == 32) cbPixel = 4;
			else { Debug.Assert(false); return false; }

			int iMax = iOffset + (w * h * cbPixel);
			for(int i = iOffset; i < iMax; i += cbPixel)
				RecolorColorBgr(pb, i, fHue);

			return true;
		}

		private static void RecolorColorBgr(byte[] pb, int iOffset, float fHue)
		{
			Color clr = Color.FromArgb(pb[iOffset + 2], pb[iOffset + 1],
				pb[iOffset]);

			float fH, fS, fV;
			UIUtil.ColorToHsv(clr, out fH, out fS, out fV);

			clr = UIUtil.ColorFromHsv(fHue, fS, fV);

			pb[iOffset] = clr.B;
			pb[iOffset + 1] = clr.G;
			pb[iOffset + 2] = clr.R;
		}
	}
}
