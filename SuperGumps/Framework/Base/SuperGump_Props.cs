#region Header
//   Vorspire    _,-'/-'/  SuperGump_Props.cs
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
#endregion

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump
	{
		public virtual void AddProperties(Item item)
		{
			if (item == null || item.Deleted)
			{
				return;
			}

			if (User.IsOnline())
			{
				item.Delta(ItemDelta.Update | ItemDelta.Properties);
				item.SendInfoTo(User.NetState, true);
			}

			AddProperties(item.Serial);
		}

		public virtual void AddProperties(Mobile mob)
		{
			if (mob == null || mob.Deleted)
			{
				return;
			}

			if (User.IsOnline())
			{
				mob.Delta(MobileDelta.Noto | MobileDelta.Properties);
				mob.SendPropertiesTo(User);
			}

			AddProperties(mob.Serial);
		}

		public virtual void AddProperties(Serial serial)
		{
			Add(new GumpOPL(serial));
		}
	}
}