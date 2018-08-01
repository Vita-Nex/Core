#region Header
//   Vorspire    _,-'/-'/  NetStateExt.cs
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
#endregion

namespace Server.Network
{
	public static class NetStateExtUtility
	{
		private static readonly ClientVersion _70500 = new ClientVersion("7.0.50.0");
		private static readonly ClientVersion _70610 = new ClientVersion("7.0.61.0");

		public static bool IsEnhanced(this NetState state)
		{
			if (state == null || state.Version == null)
			{
				return false;
			}

			bool ec;

			if (!state.GetPropertyValue("IsEnhancedClient", out ec))
			{
				ec = state.Version.Major >= 67 || state.Version.Type == ClientType.UOTD;
			}

			return ec;
		}

		public static bool SupportsUltimaStore(this NetState state)
		{
			return state.Version >= _70500;
		}

		public static bool SupportsEndlessJourney(this NetState state)
		{
			return state.Version >= _70610;
		}
	}
}