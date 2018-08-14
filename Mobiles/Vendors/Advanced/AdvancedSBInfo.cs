#region Header
//   Vorspire    _,-'/-'/  AdvancedSBInfo.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Server;
using Server.Commands;
using Server.Mobiles;
#endregion

namespace VitaNex.Mobiles
{
	public class AdvancedSBInfo : SBInfo
	{
		public IAdvancedVendor Vendor { get; private set; }

		private readonly List<GenericBuyInfo> _BuyInfo;
		public sealed override List<GenericBuyInfo> BuyInfo { get { return _BuyInfo; } }

		private readonly AdvancedSellInfo _SellInfo;
		public sealed override IShopSellInfo SellInfo { get { return _SellInfo; } }

		public int StockCount { get { return _BuyInfo.Count; } }
		public int OrderCount { get { return _SellInfo.Table.Count; } }

		public AdvancedSBInfo(IAdvancedVendor vendor)
		{
			Vendor = vendor;

			_BuyInfo = new List<GenericBuyInfo>();
			_SellInfo = new AdvancedSellInfo(this);

			InitBuyInfo();
		}

		public virtual void InitBuyInfo()
		{ }

		public void AddStock<TObj>(int price, string name = null, int amount = 100, object[] args = null)
		{
			AddStock(typeof(TObj), price, name, amount, args);
		}

		public virtual void AddStock(Type type, int price, string name = null, int amount = 100, object[] args = null)
		{
			AddStock(new AdvancedBuyInfo(this, type, price, name, amount, args));
		}

		public virtual void AddStock(AdvancedBuyInfo buy)
		{
			_BuyInfo.Add(buy);
		}

		public void RemoveStock<TObj>()
		{
			RemoveStock(typeof(TObj));
		}

		public virtual void RemoveStock(Type type)
		{
			_BuyInfo.OfType<AdvancedBuyInfo>().Where(b => b.Type.TypeEquals(type)).ForEach(RemoveStock);
		}

		public virtual void RemoveStock(AdvancedBuyInfo buy)
		{
			_BuyInfo.Remove(buy);
		}

		public void AddOrder<TObj>(int price)
		{
			AddOrder(typeof(TObj), price);
		}

		public virtual void AddOrder(Type type, int price)
		{
			_SellInfo.Add(type, price);
		}

		public void RemoveOrder<TObj>()
		{
			RemoveOrder(typeof(TObj));
		}

		public virtual void RemoveOrder(Type type)
		{
			_SellInfo.Remove(type);
		}

		public IEnumerable<KeyValuePair<Type, int>> EnumerateOrders()
		{
			return EnumerateOrders(null);
		}

		public IEnumerable<KeyValuePair<Type, int>> EnumerateOrders(Func<Type, int, bool> predicate)
		{
			return predicate != null ? _SellInfo.Table.Where(kv => predicate(kv.Key, kv.Value)) : _SellInfo.Table;
		}

		public IEnumerable<AdvancedBuyInfo> EnumerateStock()
		{
			return EnumerateStock(null);
		}

		public IEnumerable<AdvancedBuyInfo> EnumerateStock(Func<AdvancedBuyInfo, bool> predicate)
		{
			return predicate != null ? _BuyInfo.OfType<AdvancedBuyInfo>().Where(predicate) : _BuyInfo.OfType<AdvancedBuyInfo>();
		}
	}

	public class DynamicBuyInfo : GenericBuyInfo
	{
		private readonly bool _Init;

		public Item Item { get; private set; }

		public override int ControlSlots
		{
			get
			{
				int slots;

				if (Item.GetPropertyValue("ControlSlots", out slots))
				{
					return slots;
				}

				return 0;
			}
		}

		public DynamicBuyInfo(Item item)
			: base(item.ResolveName(), item.GetType(), -1, item.Amount, item.ItemID, item.Hue)
		{
			Item = item;

			_Init = true;
		}

		public DynamicBuyInfo(GenericReader reader)
			: base("Item", typeof(Item), -1, 0, 0, 0)
		{
			Deserialize(reader);

			_Init = true;
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.SetVersion(0);

			writer.Write(Item);

			writer.WriteType(Type);

			writer.Write(Name);
			writer.Write(ItemID);
			writer.Write(Hue);

			writer.Write(Amount);
			writer.Write(MaxAmount);

			var scalar = PriceScalar;

			PriceScalar = 0;

			var price = Price;

			PriceScalar = scalar;

			writer.Write(price);
			writer.Write(scalar);
		}

		public virtual void Deserialize(GenericReader reader)
		{
			reader.GetVersion();

			Item = reader.ReadItem();

			Type = reader.ReadType();

			Name = reader.ReadString();
			ItemID = reader.ReadInt();
			Hue = reader.ReadInt();

			Amount = reader.ReadInt();
			MaxAmount = reader.ReadInt();

			Price = reader.ReadInt();
			PriceScalar = reader.ReadInt();
		}

		public void Update()
		{
			Update(Item);
		}

		public void Update(Item item)
		{
			var changed = Item != item;

			if (changed)
			{
				DeleteDisplayEntity();
			}

			Item = item;

			if (Item != null)
			{
				if (changed)
				{
					Type = Item.GetType();
					Price = -1;
					Args = null;
				}

				Name = Item.ResolveName();
				Amount = MaxAmount = Item.Amount;
				ItemID = Item.ItemID;
				Hue = Item.Hue;
			}
			else
			{
				Type = null;
				Price = -1;
				Args = null;

				Name = null;
				Amount = MaxAmount = 0;
				ItemID = 0;
				Hue = 0;
			}
		}

		public void Free()
		{
			Update(null);
		}

		public override IEntity GetEntity()
		{
			if (Item != null && !Item.Deleted)
			{
				return Dupe.DupeItem(Item);
			}

			return _Init ? base.GetEntity() : null;
		}
	}

	public class AdvancedBuyInfo : GenericBuyInfo
	{
		public AdvancedSBInfo Parent { get; private set; }

		public virtual int Slots { get; set; }
		public sealed override int ControlSlots { get { return Slots; } }

		public AdvancedBuyInfo(
			AdvancedSBInfo parent,
			Type type,
			int price,
			string name = null,
			int amount = 100,
			object[] args = null)
			: base(name, type, price, amount, 0, 0, args)
		{
			Parent = parent;

			var e = GetDisplayEntity();

			if (e is Mobile)
			{
				var m = (Mobile)e;

				if (String.IsNullOrWhiteSpace(name))
				{
					Name = m.RawName ?? type.Name.SpaceWords();
				}
				else
				{
					m.RawName = name;
				}

				ItemID = ShrinkTable.Lookup(m);
				Hue = m.Hue;

				if (m is BaseCreature)
				{
					Slots = ((BaseCreature)m).ControlSlots;
				}
			}
			else if (e is Item)
			{
				var i = (Item)e;

				if (String.IsNullOrWhiteSpace(name))
				{
					Name = i.ResolveName();
				}
				else
				{
					i.Name = name;
				}

				ItemID = i.ItemID;
				Hue = i.Hue;

				int slots;

				if (i.GetPropertyValue("ControlSlots", out slots))
				{
					Slots = slots;
				}
			}
			else if (String.IsNullOrWhiteSpace(name))
			{
				Name = type.Name.SpaceWords();
			}
		}
	}

	public class AdvancedSellInfo : GenericSellInfo
	{
		private static readonly FieldInfo _TableField =
			typeof(GenericSellInfo).GetField("m_Table", BindingFlags.Instance | BindingFlags.NonPublic) ??
			typeof(GenericSellInfo).GetField("_Table", BindingFlags.Instance | BindingFlags.NonPublic);

		public AdvancedSBInfo Parent { get; private set; }
		public Dictionary<Type, int> Table { get; private set; }

		public AdvancedSellInfo(AdvancedSBInfo parent)
		{
			Parent = parent;

			Table = _TableField.GetValue(this) as Dictionary<Type, int> ?? new Dictionary<Type, int>();
		}

		public void Add<TObj>(int price)
		{
			Add(typeof(TObj), price);
		}

		public new virtual void Add(Type type, int price)
		{
			base.Add(type, price);
		}

		public bool Remove<TObj>()
		{
			return Remove(typeof(TObj));
		}

		public virtual bool Remove(Type type)
		{
			return Table.Remove(type);
		}
	}
}