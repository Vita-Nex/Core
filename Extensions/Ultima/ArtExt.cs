#region Header
//   Vorspire    _,-'/-'/  ArtExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

// Optional: Uncomment if you have Ultima.dll installed
//#define UltimaSDK

#region References
#if UltimaSDK
using Server.Items;
using Server;

using System.Drawing;

using Ultima;
#else
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;

using Server;
using Server.Items;

using VitaNex;
#endif

using Size = System.Drawing.Size;
#endregion

namespace Ultima
{
	public static class ArtExtUtility
	{
#if !UltimaSDK
		private static readonly MethodInfo _UltimaArtGetStatic;
		private static readonly MethodInfo _UltimaArtMeasure;

		static ArtExtUtility()
		{
			try
			{
				var a = Assembly.LoadFrom("Ultima.dll");

				if (a == null)
				{
					return;
				}

				var t = a.GetType("Ultima.Art");

				if (t != null)
				{
					_UltimaArtGetStatic =
						t.GetMethods(BindingFlags.Static | BindingFlags.Public)
						 .FirstOrDefault(m => m.Name == "GetStatic" && m.ReturnType.IsEqual<Bitmap>() && m.GetParameters().Length == 3);

					_UltimaArtMeasure = t.GetMethod("Measure", BindingFlags.Static | BindingFlags.Public);
				}
			}
			catch (Exception e)
			{
				VitaNexCore.ToConsole("UltimaArt could not load Ultima.dll or a member of Ultima.Art:");
				VitaNexCore.ToConsole(e);
			}
		}

		private static Bitmap ExternalGetStatic(int index, out bool patched, bool checkMaxID = true)
		{
			var param = new object[] {index, false, checkMaxID};

			var img = (Bitmap)_UltimaArtGetStatic.Invoke(null, param);

			patched = (bool)param[1];

			return img;
		}

		// ReSharper disable UnusedParameter.Local
		private static Bitmap InternalGetStatic(int index, out bool patched, bool checkMaxID = true)
		{
			patched = false;
			return null;
		}

		// ReSharper restore UnusedParameter.Local

		public static Bitmap GetStatic(int index, bool checkMaxID = true)
		{
			bool patched;
			return GetStatic(index, out patched, checkMaxID);
		}

		public static Bitmap GetStatic(int index, out bool patched, bool checkMaxID = true)
		{
			if (_UltimaArtGetStatic != null)
			{
				return ExternalGetStatic(index, out patched, checkMaxID);
			}

			return InternalGetStatic(index, out patched, checkMaxID);
		}

		private static void ExternalMeasure(Bitmap img, out int xMin, out int yMin, out int xMax, out int yMax)
		{
			var param = new object[] {img, 0, 0, 0, 0};

			_UltimaArtMeasure.Invoke(null, param);

			xMin = (int)param[1];
			yMin = (int)param[2];
			xMax = (int)param[3];
			yMax = (int)param[4];
		}

		private static void InternalMeasure(Bitmap img, out int xMin, out int yMin, out int xMax, out int yMax)
		{
			xMin = yMin = 0;

			if (img != null)
			{
				xMax = img.Width;
				yMax = img.Height;
			}
			else
			{
				xMax = yMax = 44;
			}
		}

		public static void Measure(Bitmap img, out int xMin, out int yMin, out int xMax, out int yMax)
		{
			if (_UltimaArtMeasure != null)
			{
				ExternalMeasure(img, out xMin, out yMin, out xMax, out yMax);
			}
			else
			{
				InternalMeasure(img, out xMin, out yMin, out xMax, out yMax);
			}
		}
#else
		public static Bitmap GetStatic(int index, bool checkMaxID = true)
		{
			bool patched;
			return GetStatic(index, out patched, checkMaxID);
		}

		public static Bitmap GetStatic(int index, out bool patched, bool checkMaxID = true)
		{
			return Art.GetStatic(index, out patched, checkMaxID);
		}

		public static void Measure(Bitmap img, out int xMin, out int yMin, out int xMax, out int yMax)
		{
			Art.Measure(img, out xMin, out yMin, out xMax, out yMax);
		}
#endif

		public static Point GetImageOffset(this Item item)
		{
			return GetImageOffset(item == null ? 0 : item.ItemID);
		}

		public static Point GetImageOffset(int id)
		{
			var b = GetImageSize(id);

			int x = 0, y = 0;

			if (b.Width > 44)
			{
				x -= (b.Width - 44) / 2;
			}
			else if (b.Width < 44)
			{
				x += (44 - b.Width) / 2;
			}

			if (b.Height > 44)
			{
				y -= (b.Height - 44);
			}
			else if (b.Height < 44)
			{
				y += (44 - b.Height);
			}

			return new Point(x, y);
		}

		public static int GetImageWidth(int id)
		{
			var img = GetStatic(id);

			if (img == null)
			{
				return 44;
			}

			return img.Width;
		}

		public static int GetImageWidth(this Item item)
		{
			if (item == null || item is BaseMulti)
			{
				return 44;
			}

			return GetImageWidth(item.ItemID);
		}

		public static int GetImageHeight(int id)
		{
			var img = GetStatic(id);

			if (img == null)
			{
				return 44;
			}

			return img.Height;
		}

		public static int GetImageHeight(this Item item)
		{
			if (item == null || item is BaseMulti)
			{
				return 44;
			}

			return GetImageHeight(item.ItemID);
		}

		public static Size GetImageSize(int id)
		{
			var img = GetStatic(id);

			if (img == null)
			{
				return new Size(44, 44);
			}

			return new Size(img.Width, img.Height);
		}

		public static Size GetImageSize(this Item item)
		{
			if (item == null || item is BaseMulti)
			{
				return new Size(44, 44);
			}

			return GetImageSize(item.ItemID);
		}

		public static Rectangle2D GetStaticBounds(int id)
		{
			var img = GetStatic(id);

			if (img == null)
			{
				return new Rectangle2D(0, 0, 44, 44);
			}

			int xMin, xMax, yMin, yMax;
			Measure(img, out xMin, out yMin, out xMax, out yMax);

			return new Rectangle2D(new Point2D(xMin, yMin), new Point2D(xMax, yMax));
		}

		public static Rectangle2D GetStaticBounds(this Item item)
		{
			if (item == null || item is BaseMulti)
			{
				return new Rectangle2D(0, 0, 44, 44);
			}

			return GetStaticBounds(item.ItemID);
		}
	}
}