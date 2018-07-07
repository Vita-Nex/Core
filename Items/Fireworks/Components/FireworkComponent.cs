#region Header
//   Vorspire    _,-'/-'/  FireworkComponent.cs
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
using System.Text;

using Server;
using Server.Engines.Craft;
using Server.Items;
#endregion

namespace VitaNex.Items
{
	public abstract class FireworkComponent : Item, ICraftable
	{
		private Mobile _Crafter;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Mobile Crafter
		{
			get { return _Crafter; }
			set
			{
				_Crafter = value;
				InvalidateProperties();
			}
		}

		public FireworkComponent(int itemID)
			: base(itemID)
		{ }

		public FireworkComponent(Serial serial)
			: base(serial)
		{ }

		public override bool StackWith(Mobile m, Item dropped, bool playSound)
		{
			if (m == null || !(dropped is FireworkComponent) || ((FireworkComponent)dropped)._Crafter != _Crafter)
			{
				return false;
			}

			return base.StackWith(m, dropped, playSound);
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (Crafter != null)
			{
				list.Add(1050043, Crafter.RawName);
			}

			var props = new StringBuilder();

			GetProperties(props);

			if (props.Length > 0)
			{
				list.Add(props.ToString());
			}
		}

		public virtual void GetProperties(StringBuilder props)
		{ }

		public virtual int OnCraft(
			int quality,
			bool makersMark,
			Mobile m,
			CraftSystem craft,
			Type typeRes,
#if ServUO
			ITool tool,
#else
			BaseTool tool,
#endif
			CraftItem item,
			int resHue)
		{
			if (makersMark)
			{
				Crafter = m;
			}

			var context = craft.GetContext(m);

			if (context != null && context.DoNotColor)
			{
				Hue = 0;
			}
			else if (resHue > 0)
			{
				Hue = resHue;
			}

			return quality;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);

			writer.Write(_Crafter);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();

			_Crafter = reader.ReadMobile();
		}
	}
}