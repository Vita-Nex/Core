#region Header
//   Vorspire    _,-'/-'/  SuperGump_Props.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Linq;

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

			if (item.Parent != User && User.IsOnline() && GetEntries<GumpOPL>().All(o => o.Serial != item.Serial.Value))
			{
				User.Send(item.PropertyList);
			}

			AddProperties(item.Serial);
		}

		public virtual void AddProperties(Mobile mob)
		{
			if (mob == null || mob.Deleted)
			{
				return;
			}

			if (mob != User && User.IsOnline() && GetEntries<GumpOPL>().All(o => o.Serial != mob.Serial.Value))
			{
				User.Send(mob.PropertyList);
			}

			AddProperties(mob.Serial);
		}

		public virtual void AddProperties(Serial serial)
		{
			Add(new GumpOPL(serial));
		}
	}
}