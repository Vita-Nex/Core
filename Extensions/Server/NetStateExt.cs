#region Header
//   Vorspire    _,-'/-'/  NetStateExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

namespace Server.Network
{
	public static class NetStateExtUtility
	{
		public static bool IsEnhanced(this NetState state)
		{
			return state != null && state.Version != null && state.Version.Type == ClientType.UOTD;
		}
	}
}