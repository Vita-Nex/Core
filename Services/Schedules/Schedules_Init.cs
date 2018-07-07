#region Header
//   Vorspire    _,-'/-'/  Schedules_Init.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;
#endregion

namespace VitaNex.Schedules
{
	[CoreService("Schedules", "3.0.0.0", TaskPriority.High)]
	public static partial class Schedules
	{
		static Schedules()
		{
			Registry = new List<Schedule>();
		}

		private static void CSConfig()
		{
			CommandUtility.Register("Schedules", Access, e => new ScheduleListGump(e.Mobile).Send());
		}
	}
}