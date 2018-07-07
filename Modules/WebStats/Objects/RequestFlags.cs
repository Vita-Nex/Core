#region Header
//   Vorspire    _,-'/-'/  RequestFlags.cs
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

namespace VitaNex.Modules.WebStats
{
	[Flags]
	public enum WebStatsRequestFlags
	{
		None = 0x00,
		Server = 0x01,
		Stats = 0x02,

		All = ~None
	}
}