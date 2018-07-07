#region Header
//   Vorspire    _,-'/-'/  Axis.cs
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

namespace VitaNex
{
	[Flags]
	public enum Axis
	{
		None = 0x0,
		Vertical = 0x1,
		Horizontal = 0x2,

		Both = Vertical | Horizontal
	}
}