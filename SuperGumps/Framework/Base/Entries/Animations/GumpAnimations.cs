#region Header
//   Vorspire    _,-'/-'/  GumpAnimations.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;

using Server;
using Server.Gumps;
#endregion

namespace VitaNex.SuperGumps
{
	public static class GumpAnimations
	{
		public static void Shake(GumpAnimation anim)
		{
			var p = anim.State.Slice;

			p = Math.Max(0.0, Math.Min(1.0, p <= 0.5 ? p / 0.5 : (p - 0.5) / 0.5));

			var range = anim.GetArg(0, 5);

			var r = 1 + (int)(p * range);

			foreach (var e in anim.Entries)
			{
				int x, y;

				if (e.TryGetPosition(out x, out y))
				{
					x = Math.Max(0, Utility.RandomMinMax(x - r, x + r));
					y = Math.Max(0, Utility.RandomMinMax(y - r, y + r));

					e.TrySetPosition(x, y);
				}
			}
		}

		public static void GrowWidth(GumpAnimation anim)
		{
			var p = anim.State.Slice;

			foreach (var e in anim.Entries)
			{
				int w;

				if (e.TryGetWidth(out w))
				{
					e.TrySetWidth((int)Math.Ceiling(w * p));
				}
			}
		}

		public static void GrowHeight(GumpAnimation anim)
		{
			var p = anim.State.Slice;

			foreach (var e in anim.Entries)
			{
				int h;

				if (e.TryGetHeight(out h))
				{
					e.TrySetHeight((int)Math.Ceiling(h * p));
				}
			}
		}

		public static void Grow(GumpAnimation anim)
		{
			var p = anim.State.Slice;

			foreach (var e in anim.Entries)
			{
				int w, h;

				if (e.TryGetSize(out w, out h))
				{
					e.TrySetSize((int)Math.Ceiling(w * p), (int)Math.Ceiling(h * p));
				}
			}
		}

		public static void ShrinkWidth(GumpAnimation anim)
		{
			var p = 1.0 - anim.State.Slice;

			foreach (var e in anim.Entries)
			{
				int w;

				if (e.TryGetWidth(out w))
				{
					e.TrySetWidth((int)Math.Ceiling(w * p));
				}
			}
		}

		public static void ShrinkHeight(GumpAnimation anim)
		{
			var p = 1.0 - anim.State.Slice;

			foreach (var e in anim.Entries)
			{
				int h;

				if (e.TryGetHeight(out h))
				{
					e.TrySetHeight((int)Math.Ceiling(h * p));
				}
			}
		}

		public static void Shrink(GumpAnimation anim)
		{
			var p = 1.0 - anim.State.Slice;

			foreach (var e in anim.Entries)
			{
				int w, h;

				if (e.TryGetSize(out w, out h))
				{
					e.TrySetSize((int)Math.Ceiling(w * p), (int)Math.Ceiling(h * p));
				}
			}
		}
	}
}