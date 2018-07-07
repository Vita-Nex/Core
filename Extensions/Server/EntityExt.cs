#region Header
//   Vorspire    _,-'/-'/  EntityExt.cs
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
using System.Collections.Generic;
using System.Linq;

using VitaNex;
using VitaNex.Network;
#endregion

namespace Server
{
	public static class EntityExtUtility
	{
		public static ObjectPropertyList GetOPL(this IEntity e)
		{
			return ExtendedOPL.ResolveOPL(e);
		}

		public static ObjectPropertyList GetOPL(this IEntity e, Mobile viewer)
		{
			return ExtendedOPL.ResolveOPL(e, viewer);
		}

		public static string GetOPLHeader(this IEntity e)
		{
			var opl = GetOPL(e);

			if (opl != null)
			{
				return opl.DecodePropertyListHeader();
			}

			return String.Empty;
		}

		public static string GetOPLHeader(this IEntity e, ClilocLNG lng)
		{
			var opl = GetOPL(e);

			if (opl != null)
			{
				return opl.DecodePropertyListHeader(lng);
			}

			return String.Empty;
		}

		public static string GetOPLHeader(this IEntity e, Mobile viewer)
		{
			var opl = GetOPL(e, viewer);

			if (opl != null)
			{
				return opl.DecodePropertyListHeader(viewer);
			}

			return String.Empty;
		}

		public static IEnumerable<string> GetOPLStrings(this IEntity e)
		{
			var opl = GetOPL(e);

			if (opl != null)
			{
				return opl.DecodePropertyList();
			}

			return Enumerable.Empty<string>();
		}

		public static IEnumerable<string> GetOPLStrings(this IEntity e, ClilocLNG lng)
		{
			var opl = GetOPL(e);

			if (opl != null)
			{
				return opl.DecodePropertyList(lng);
			}

			return Enumerable.Empty<string>();
		}

		public static IEnumerable<string> GetOPLStrings(this IEntity e, Mobile viewer)
		{
			var opl = GetOPL(e, viewer);

			if (opl != null)
			{
				return opl.DecodePropertyList(viewer);
			}

			return Enumerable.Empty<string>();
		}

		public static string GetOPLString(this IEntity e)
		{
			return String.Join("\n", GetOPLStrings(e));
		}

		public static string GetOPLString(this IEntity e, ClilocLNG lng)
		{
			return String.Join("\n", GetOPLStrings(e, lng));
		}

		public static string GetOPLString(this IEntity e, Mobile viewer)
		{
			return String.Join("\n", GetOPLStrings(e, viewer));
		}

		public static bool IsInside(this IEntity e)
		{
			if (e != null && e.Map != null && e.Map != Map.Internal)
			{
				return e.IsInside(e.Map);
			}

			return false;
		}

		public static bool IsOutside(this IEntity e)
		{
			return !IsInside(e);
		}

		public static bool Intersects(this IEntity e, IPoint3D o)
		{
			return Block3D.Intersects(e, o);
		}
	}
}