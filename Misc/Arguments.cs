#region Header
//   Vorspire    _,-'/-'/  Arguments.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

namespace VitaNex
{
	public static class Arguments
	{
		public static readonly object[] Empty = new object[0];

		public static object[] Create(params object[] args)
		{
			return args ?? Empty;
		}
	}
}