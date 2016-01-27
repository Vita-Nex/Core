#region Header
//   Vorspire    _,-'/-'/  Reward.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Collections.Generic;

using Server;
using Server.Mobiles;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public enum PvPRewardDeliveryMethod
	{
		None,
		Custom,
		Backpack,
		Bank
	}

	public enum PvPRewardClass
	{
		None,
		Custom,
		Item
	}

	public class PvPReward : ItemTypeSelectProperty
	{
		private PvPRewardDeliveryMethod _DeliveryMethod = PvPRewardDeliveryMethod.Backpack;

		[CommandProperty(AutoPvP.Access)]
		public virtual bool Enabled { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int Amount { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public override string TypeName
		{
			get { return base.TypeName; }
			set
			{
				base.TypeName = value;

				if (InternalType != null)
				{
					if (InternalType.IsConstructableFrom(typeof(Item), Type.EmptyTypes))
					{
						Class = PvPRewardClass.Item;
						DeliveryMethod = PvPRewardDeliveryMethod.Backpack;
					}
					else
					{
						Class = PvPRewardClass.Custom;
						DeliveryMethod = PvPRewardDeliveryMethod.Custom;
					}
				}
				else
				{
					Class = PvPRewardClass.None;
					DeliveryMethod = PvPRewardDeliveryMethod.None;
				}
			}
		}

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPRewardDeliveryMethod DeliveryMethod
		{
			get { return _DeliveryMethod; }
			set
			{
				if (value != PvPRewardDeliveryMethod.None && Class == PvPRewardClass.Custom)
				{
					value = PvPRewardDeliveryMethod.Custom;
				}

				_DeliveryMethod = value;
			}
		}

		[CommandProperty(AutoPvP.Access, true)]
		public PvPRewardClass Class { get; private set; }

		public PvPReward(string type = "")
			: base(type)
		{ }

		public PvPReward(GenericReader reader)
			: base(reader)
		{ }

		public override string ToString()
		{
			return "Battle Reward";
		}

		public override void Clear()
		{
			base.Clear();

			Enabled = false;
			Amount = 1;
			Class = PvPRewardClass.None;
			_DeliveryMethod = PvPRewardDeliveryMethod.None;
		}

		public override void Reset()
		{
			base.Reset();

			Enabled = false;
			Amount = 1;
			Class = PvPRewardClass.None;
			_DeliveryMethod = PvPRewardDeliveryMethod.Backpack;
		}

		public virtual List<Item> GiveReward(PlayerMobile pm)
		{
			var items = new List<Item>();

			if (pm == null || pm.Deleted || !Enabled)
			{
				return items;
			}

			var amount = Amount;

			while (amount > 0)
			{
				var reward = CreateInstance();

				if (reward == null)
				{
					return items;
				}

				items.Add(reward);

				if (reward.Stackable)
				{
					reward.Amount = Math.Min(60000, amount);
				}

				amount -= reward.Amount;
			}

			items.ForEach(
				item =>
				{
					switch (_DeliveryMethod)
					{
						case PvPRewardDeliveryMethod.Custom:
							break;
						case PvPRewardDeliveryMethod.Bank:
							item.GiveTo(pm, GiveFlags.Bank | GiveFlags.Delete);
							break;
						case PvPRewardDeliveryMethod.Backpack:
							item.GiveTo(pm, GiveFlags.Pack | GiveFlags.Delete);
							break;
						default:
							item.GiveTo(pm, GiveFlags.PackBankDelete);
							break;
					}
				});

			items.RemoveAll(i => i == null || i.Deleted);

			return items;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			var version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
					writer.Write(Amount);
					goto case 0;
				case 0:
				{
					writer.Write(Enabled);
					writer.WriteFlag(Class);
					writer.WriteFlag(_DeliveryMethod);
				}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					Amount = reader.ReadInt();
					goto case 0;
				case 0:
				{
					Enabled = reader.ReadBool();
					Class = reader.ReadFlag<PvPRewardClass>();
					_DeliveryMethod = reader.ReadFlag<PvPRewardDeliveryMethod>();
				}
					break;
			}

			if (version < 1)
			{
				Amount = 1;
			}
		}
	}
}