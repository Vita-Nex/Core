#region Header
//   Vorspire    _,-'/-'/  EntityExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
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
	}
}