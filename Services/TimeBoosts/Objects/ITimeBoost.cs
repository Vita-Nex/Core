#region Header
//   Vorspire    _,-'/-'/  ITimeBoost.cs
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

namespace VitaNex.TimeBoosts
{
	public interface ITimeBoost
	{
		int RawValue { get; }
		TimeSpan Value { get; }

		string Desc { get; }
		string Name { get; }
		int Hue { get; }
	}
}