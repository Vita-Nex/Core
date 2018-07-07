#region Header
//   Vorspire    _,-'/-'/  AddonExt.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Server.Items;
#endregion

namespace Server
{
	public static class AddonExtUtility
	{
		static AddonExtUtility()
		{
			ComponentsCache = new Dictionary<int, MultiComponentList>();
		}

		public static Dictionary<int, MultiComponentList> ComponentsCache { get; private set; }

		public static int ComputeHash(IAddon addon)
		{
			unchecked
			{
				var hash = -1;

				hash = (hash * 397) ^ addon.GetTypeHashCode();

				IList comp;

				if (addon.GetPropertyValue("Components", out comp))
				{
					hash = (hash * 397) ^ comp.Count;

					hash = comp.Cast<Item>().Aggregate(hash, (h, c) => (h * 397) ^ c.GetTypeHashCode());
					hash = comp.Cast<Item>().Aggregate(hash, (h, c) => (h * 397) ^ c.ItemID);
				}

				return hash;
			}
		}

		public static MultiComponentList GetComponents(this IAddon addon)
		{
			if (addon == null)
			{
				return null;
			}

			var hash = ComputeHash(addon);

			var mcl = ComponentsCache.GetValue(hash);

			if (mcl != null)
			{
				return mcl;
			}

			mcl = new MultiComponentList(MultiComponentList.Empty);

			int x1 = 0, y1 = 0, x2 = 0, y2 = 0;

			IList comp;

			if (addon.GetPropertyValue("Components", out comp))
			{
				Point3D off;

				foreach (var c in comp)
				{
					if (c.GetPropertyValue("Offset", out off))
					{
						x1 = Math.Min(off.X, x1);
						y1 = Math.Min(off.Y, y1);

						x2 = Math.Max(off.X, x2);
						y2 = Math.Max(off.Y, y2);
					}
				}
			}

			mcl.Resize(1 + (x2 - x1), 1 + (y2 - y1));

			if (comp != null)
			{
				Point3D off;

				foreach (var c in comp.OfType<Item>())
				{
					if (c.GetPropertyValue("Offset", out off))
					{
						off = off.Clone3D(Math.Abs(x1), Math.Abs(y1));

						mcl.Add(c.ItemID, off.X, off.Y, off.Z);
					}
				}
			}

			if (addon is Item)
			{
				((Item)addon).Delete();
			}

			return ComponentsCache[hash] = mcl;
		}
	}
}