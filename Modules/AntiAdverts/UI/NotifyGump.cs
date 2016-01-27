#region Header
//   Vorspire    _,-'/-'/  NotifyGump.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server;

using VitaNex.Notify;
#endregion

namespace VitaNex.Modules.AntiAdverts
{
	public sealed class AntiAdvertNotifyGump : NotifyGump
	{
		private static void InitSettings(NotifySettings settings)
		{
			settings.Name = "Advertising Reports";
			settings.CanIgnore = true;
			settings.Access = AntiAdverts.Access;
		}

		public AntiAdvertNotifyGump(Mobile user, string html)
			: base(user, html)
		{ }
	}
}