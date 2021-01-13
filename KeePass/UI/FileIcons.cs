/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2021 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

using KeePass.Native;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.UI
{
	internal static class FileIcons
	{
		private const char FiTypePath = 'P';
		private const char FiTypeExt = 'E';

		private const string FiImageListTag = "FileIcons";

		// Images may be used/owned by other components
		private static readonly Dictionary<string, Image> g_dCache =
			new Dictionary<string, Image>();

		private static string GetCacheItemKey(char chType, string strID, Size sz)
		{
			StringBuilder sb = new StringBuilder();
			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;

			sb.Append(chType);
			// sb.Append(':');
			sb.Append((strID ?? string.Empty).Trim());
			sb.Append('_');
			sb.Append(sz.Width.ToString(nfi));
			sb.Append('x');
			sb.Append(sz.Height.ToString(nfi));

			return sb.ToString();
		}

		private static Size GetSizeOrSmall(Size? osz)
		{
			return (osz.HasValue ? osz.Value : UIUtil.GetSmallIconSize());
		}

		public static Image GetImageForName(string strName, Size? osz)
		{
			if(strName == null) { Debug.Assert(false); strName = string.Empty; }

			string strExt = UrlUtil.GetExtension(strName);
			return GetImageForExtension(strExt, osz);
		}

		public static Image GetImageForExtension(string strExt, Size? osz)
		{
			strExt = (strExt ?? string.Empty).Trim().ToLowerInvariant();
			Size sz = GetSizeOrSmall(osz);

			string strKey = GetCacheItemKey(FiTypeExt, strExt, sz);
			Image img;
			if(g_dCache.TryGetValue(strKey, out img)) return img;

			try
			{
				if(strExt.Length != 0)
				{
					string strTemp = Program.TempFilesPool.GetTempFileName(strExt);
					File.WriteAllBytes(strTemp, MemUtil.EmptyByteArray);

					img = GetImageForPath(strTemp, sz, false, false);

					Program.TempFilesPool.Delete(strTemp);
				}
			}
			catch(Exception) { Debug.Assert(false); }

			if(img == null)
				img = GetImageForPath(string.Empty, sz, true, false);

			g_dCache[strKey] = img;
			return img;
		}

		public static Image GetImageForPath(string strPath, Size? osz,
			bool bAllowCache, bool bNewImageObject)
		{
			if(strPath == null) { Debug.Assert(false); strPath = string.Empty; }
			Size sz = GetSizeOrSmall(osz);

			Image img = null;
			bool bImgIsNew = false;

			string strKey = GetCacheItemKey(FiTypePath, strPath, sz);
			if(bAllowCache && g_dCache.TryGetValue(strKey, out img))
			{
				if(bNewImageObject) img = (img.Clone() as Image);
				return img;
			}
			Debug.Assert(img == null);

			if(strPath.Length == 0)
			{
				img = Properties.Resources.B16x16_Binary;
				bImgIsNew = false;
			}

			try
			{
				if((img == null) && !NativeLib.IsUnix())
				{
					string strDisplayName;
					NativeMethods.SHGetFileInfo(strPath, sz.Width, sz.Height,
						out img, out strDisplayName);
					bImgIsNew = true;
				}
			}
			catch(Exception) { Debug.Assert(false); }

			if((img == null) && !MonoWorkarounds.IsRequired(100003))
			{
				img = UIUtil.GetFileIcon(strPath, sz.Width, sz.Height);
				bImgIsNew = true;
			}

			if(img == null)
			{
				BinaryDataClass bdc = BinaryDataClassifier.ClassifyUrl(strPath);

				if((bdc == BinaryDataClass.Text) || (bdc == BinaryDataClass.RichText))
					img = Properties.Resources.B16x16_ASCII;
				else if(bdc == BinaryDataClass.Image)
					img = Properties.Resources.B16x16_Spreadsheet;
				else if(bdc == BinaryDataClass.WebDocument)
					img = Properties.Resources.B16x16_HTML;
				else if(strPath.EndsWith(".exe", StrUtil.CaseIgnoreCmp))
					img = Properties.Resources.B16x16_Make_KDevelop;
				else
					img = Properties.Resources.B16x16_Binary;

				bImgIsNew = false;
			}

			if(img == null) { Debug.Assert(false); return null; }

			if((img.Width != sz.Width) || (img.Height != sz.Height))
			{
				Image imgSc = GfxUtil.ScaleImage(img, sz.Width, sz.Height,
					ScaleTransformFlags.UIIcon);
				if(bImgIsNew) img.Dispose();
				img = imgSc;
			}

			if(bAllowCache) g_dCache[strKey] = img;

			if(bNewImageObject && (bAllowCache || !bImgIsNew))
				img = (img.Clone() as Image);

			return img;
		}

		internal static void UpdateImages(ListView lv,
			GFunc<ListViewItem, string> fItemToName)
		{
			if(lv == null) { Debug.Assert(false); return; }

			Size sz = GetSizeOrSmall(null);

			ImageList ilToDispose = lv.SmallImageList;
			lv.BeginUpdate();

			List<Image> lImages = new List<Image>();
			foreach(ListViewItem lvi in lv.Items)
			{
				if(lvi == null) { Debug.Assert(false); continue; }

				string strName;
				if(fItemToName != null) strName = fItemToName(lvi);
				else strName = lvi.Text;

				Image img = GetImageForName(strName, sz);
				if(img == null) { Debug.Assert(false); continue; }

				int iImage = -1;
				for(int i = 0; i < lImages.Count; ++i)
				{
					if(object.ReferenceEquals(lImages[i], img))
					{
						iImage = i;
						break;
					}
				}
				if(iImage < 0)
				{
					iImage = lImages.Count;
					lImages.Add(img);
				}

				lvi.ImageIndex = iImage;
			}

			if(lImages.Count != 0)
			{
				ImageList il = UIUtil.BuildImageListUnscaled(lImages,
					sz.Width, sz.Height);
#if DEBUG
				il.Tag = FiImageListTag;
#endif
				lv.SmallImageList = il;
			}
			else if(ilToDispose != null)
				lv.SmallImageList = null; // Release previous ImageList

			lv.EndUpdate();
			if(ilToDispose != null)
			{
				Debug.Assert((ilToDispose.Tag as string) == FiImageListTag);
				ilToDispose.Dispose();
			}
		}

		internal static void ReleaseImages(ListView lv)
		{
			if(lv == null) { Debug.Assert(false); return; }

			ImageList il = lv.SmallImageList;
			if(il == null) return;

			lv.BeginUpdate();

			foreach(ListViewItem lvi in lv.Items) lvi.ImageIndex = -1;
			lv.SmallImageList = null;

			lv.EndUpdate();

			Debug.Assert((il.Tag as string) == FiImageListTag);
			il.Dispose();
		}
	}
}
