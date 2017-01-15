#region Header
//   Vorspire    _,-'/-'/  AdvancedVendor.cs
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
using System.Drawing;
using System.Linq;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

using VitaNex.Network;
#endregion

namespace VitaNex.Mobiles
{
	public abstract class AdvancedVendor : BaseVendor, IAdvancedVendor
	{
		public static event Action<AdvancedVendor> OnCreated;
		public static event Action<AdvancedSBInfo> OnInit;

	    public override bool CanTeach { get { return false; } }

	    public static List<AdvancedVendor> Instances { get; private set; }

		static AdvancedVendor()
		{
			Instances = new List<AdvancedVendor>();
		}

		private readonly List<SBInfo> _SBInfos = new List<SBInfo>();
		protected sealed override List<SBInfo> SBInfos { get { return _SBInfos; } }

		public AdvancedSBInfo AdvancedStock { get; private set; }

		private DateTime _NextYell = DateTime.UtcNow.AddSeconds(Utility.RandomMinMax(30, 120));

		[CommandProperty(AccessLevel.Administrator)]
		public int Discount { get; set; }

		[CommandProperty(AccessLevel.Administrator)]
		public bool DiscountEnabled { get; set; }

		[CommandProperty(AccessLevel.Administrator)]
		public bool DiscountYell { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public ObjectProperty CashProperty { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public TypeSelectProperty<object> CashType { get; set; }

		private TextDefinition _CashName;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public TextDefinition CashName
		{
			get { return _CashName; }
			set
			{
				_CashName = value;
				InvalidateProperties();
			}
		}

		private TextDefinition _CashAbbr;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public TextDefinition CashAbbr
		{
			get { return _CashAbbr; }
			set
			{
				_CashAbbr = value;
				InvalidateProperties();
			}
		}

		private bool _ShowCashName;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool ShowCashName
		{
			get { return _ShowCashName; }
			set
			{
				_ShowCashName = value;
				InvalidateProperties();
			}
		}

		private bool _Trading;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Trading
		{
			get { return _Trading; }
			set
			{
				_Trading = value;
				InvalidateProperties();
			}
		}

		public override VendorShoeType ShoeType
		{
			get { return Utility.RandomBool() ? VendorShoeType.ThighBoots : VendorShoeType.Boots; }
		}

		public virtual Race DefaultRace { get { return Race.Human; } }

		public virtual Race[] RequiredRaces { get { return null; } }

		public AdvancedVendor(
			string title,
			Type cashType,
			TextDefinition cashName,
			TextDefinition cashAbbr = null,
			bool showCashName = true)
			: this(title)
		{
			CashProperty = new ObjectProperty();
			CashType = cashType ?? typeof(Gold);
			CashName = cashName ?? "Gold";
			CashAbbr = cashAbbr ?? String.Empty;
			ShowCashName = showCashName;
		}

		public AdvancedVendor(
			string title,
			string cashProp,
			TextDefinition cashName,
			TextDefinition cashAbbr = null,
			bool showCashName = true)
			: this(title)
		{
			CashProperty = new ObjectProperty(cashProp);
			CashType = typeof(ObjectProperty);
			CashName = cashName ?? "Credits";
			CashAbbr = cashAbbr ?? String.Empty;
			ShowCashName = showCashName;
		}

		private AdvancedVendor(string title)
			: base(title)
		{
			Instances.Add(this);

			Trading = true;

			Female = Utility.RandomBool();
			Name = NameList.RandomName(Female ? "female" : "male");

			Race = DefaultRace;

			if (OnCreated != null)
			{
				Timer.DelayCall(v => OnCreated(v), this);
			}
		}

		public AdvancedVendor(Serial serial)
			: base(serial)
		{
			Instances.Add(this);
		}

		public override int GetPriceScalar()
		{
			var scalar = CashType.TypeEquals<Gold>() ? base.GetPriceScalar() : 100;

			if (DiscountEnabled)
			{
				scalar -= Math.Max(0, Math.Min(100, Discount));
			}

			return Math.Max(0, scalar);
		}

		protected override void OnRaceChange(Race oldRace)
		{
			base.OnRaceChange(oldRace);

			Items.ForEachReverse(
				item =>
				{
					if (item != null && !item.Deleted && !(item is Container) && !(item is IMount) && !(item is IMountItem) &&
						item.IsEquipped())
					{
						item.Delete();
					}
				});

			Hue = /*FaceHue =*/ Race.RandomSkinHue();

			//FaceItemID = Race.RandomFace(this);
			//FaceHue = Hue;

			HairItemID = Race.RandomHair(this);
			HairHue = Race.RandomHairHue();

			FacialHairItemID = Race.RandomFacialHair(this);
			FacialHairHue = HairHue;

			InitOutfit();
		}

		public virtual void ResolveCurrency(out Type type, out TextDefinition name)
		{
			type = CashType;
			name = CashName;
		}

		public virtual object GetCashObject(Mobile m)
		{
			return m;
		}

#if ServUO
		/*public override void InitOutfit()
		{
			if (Race == Race.Gargoyle)
			{
				InitGargOutfit();
				return;
			}

			base.InitOutfit();
		}*/
#endif

		public sealed override void InitSBInfo()
		{
			_SBInfos.ForEach(sb => sb.BuyInfo.ForEach(b => b.DeleteDisplayEntity()));
			_SBInfos.Clear();

			_SBInfos.Add(AdvancedStock = new AdvancedSBInfo(this));

			InitBuyInfo();

			if (OnInit != null)
			{
				OnInit(AdvancedStock);
			}
		}

		protected abstract void InitBuyInfo();

		public void AddStock<TObj>(int price, string name = null, int amount = 100, object[] args = null)
		{
			AddStock(typeof(TObj), price, name, amount, args);
		}

		public virtual void AddStock(Type type, int price, string name = null, int amount = 100, object[] args = null)
		{
			AdvancedStock.AddStock(type, price, name, amount, args);
		}

		public void RemoveStock<TObj>()
		{
			RemoveStock(typeof(TObj));
		}

		public virtual void RemoveStock(Type type)
		{
			AdvancedStock.RemoveStock(type);
		}

		public void AddOrder<TObj>(int price)
		{
			AddOrder(typeof(TObj), price);
		}

		public virtual void AddOrder(Type type, int price)
		{
			AdvancedStock.AddOrder(type, price);
		}

		public void RemoveOrder<TObj>()
		{
			RemoveOrder(typeof(TObj));
		}

		public virtual void RemoveOrder(Type type)
		{
			AdvancedStock.RemoveOrder(type);
		}

		public override bool CheckVendorAccess(Mobile m)
		{
			if (m == null || m.Deleted)
			{
				return false;
			}

			if (m.AccessLevel >= AccessLevel.GameMaster)
			{
				return true;
			}

			if (!Trading || !DesignContext.Check(m))
			{
				return false;
			}

			var races = RequiredRaces;

			if (races != null && races.Length > 0 && (m.Race == null || !races.Contains(m.Race)))
			{
				return false;
			}

			return base.CheckVendorAccess(m);
		}

		public override void OnThink()
		{
			base.OnThink();

			if (Deleted || Map == null || Map == Map.Internal || !DiscountEnabled || !DiscountYell || Discount <= 0 ||
				DateTime.UtcNow <= _NextYell)
			{
				return;
			}

			Yell("Sale! {0}% Off!", Discount.ToString("#,0"));
			_NextYell = DateTime.UtcNow.AddSeconds(Utility.RandomMinMax(20, 120));
		}

		public override void OnDelete()
		{
			base.OnDelete();

			Instances.Remove(this);
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			Instances.Remove(this);
		}

		private GenericBuyInfo LookupDisplayObject(object obj)
		{
			return GetBuyInfo().OfType<GenericBuyInfo>().FirstOrDefault(gbi => gbi.GetDisplayEntity() == obj);
		}

		private void ProcessSinglePurchase(
			BuyItemResponse buy,
			IBuyItemInfo bii,
			ICollection<BuyItemResponse> validBuy,
			ref int controlSlots,
			ref bool fullPurchase,
			ref int totalCost)
		{
			if (!Trading || CashType == null || !CashType.IsNotNull)
			{
				return;
			}

			var amount = buy.Amount;

			if (amount > bii.Amount)
			{
				amount = bii.Amount;
			}

			if (amount <= 0)
			{
				return;
			}

			var slots = bii.ControlSlots * amount;

			if (controlSlots >= slots)
			{
				controlSlots -= slots;
			}
			else
			{
				fullPurchase = false;
				return;
			}

			totalCost += bii.Price * amount;
			validBuy.Add(buy);
		}

		private void ProcessValidPurchase(int amount, IBuyItemInfo bii, Mobile buyer, Container cont)
		{
			if (!Trading || CashType == null || !CashType.IsNotNull)
			{
				return;
			}

			if (amount > bii.Amount)
			{
				amount = bii.Amount;
			}

			if (amount < 1)
			{
				return;
			}

			bii.Amount -= amount;

			var o = bii.GetEntity();

			if (o is Item)
			{
				var item = (Item)o;

				if (item.Stackable)
				{
					item.Amount = amount;

					if (cont == null || !cont.TryDropItem(buyer, item, false))
					{
						item.MoveToWorld(buyer.Location, buyer.Map);
					}
				}
				else
				{
					item.Amount = 1;

					if (cont == null || !cont.TryDropItem(buyer, item, false))
					{
						item.MoveToWorld(buyer.Location, buyer.Map);
					}

					for (var i = 1; i < amount; i++)
					{
						item = (Item)bii.GetEntity();

						if (item == null)
						{
							continue;
						}

						item.Amount = 1;

						if (cont == null || !cont.TryDropItem(buyer, item, false))
						{
							item.MoveToWorld(buyer.Location, buyer.Map);
						}
					}
				}
			}
			else if (o is Mobile)
			{
				var m = (Mobile)o;

				m.Direction = (Direction)Utility.Random(8);
				m.MoveToWorld(buyer.Location, buyer.Map);
				m.PlaySound(m.GetIdleSound());

				if (m is BaseCreature)
				{
					((BaseCreature)m).SetControlMaster(buyer);
				}

				for (var i = 1; i < amount; ++i)
				{
					m = (Mobile)bii.GetEntity();

					if (m == null)
					{
						continue;
					}

					m.Direction = (Direction)Utility.Random(8);
					m.MoveToWorld(buyer.Location, buyer.Map);

					if (m is BaseCreature)
					{
						((BaseCreature)m).SetControlMaster(buyer);
					}
				}
			}
		}

		public override void VendorSell(Mobile m)
		{
			if (!IsActiveBuyer)
			{
				return;
			}

			if (!m.CheckAlive())
			{
				return;
			}

			if (!CheckVendorAccess(m))
			{
				Say("I can't serve you! Company policy.");
				//Say(501522); // I shall not treat with scum like thee!
				return;
			}

			var pack = m.Backpack;

			if (pack == null)
			{
				return;
			}

			var info = GetSellInfo();

			var table = new List<SellItemState>();

			foreach (var ssi in info.Where(ssi => ssi != null && ssi.Types != null))
			{
				var range = pack.FindItemsByType(ssi.Types).Where(i => CanSellItem(i, ssi));

				table.AddRange(range.Select(item => new SellItemState(item, ssi.GetSellPriceFor(item), ssi.GetNameFor(item))));
			}

			if (table.Count > 0)
			{
				SendPacksTo(m);

				m.Send(new VendorSellList(this, table));
			}
			else
			{
				Say(true, "You have nothing I would be interested in.");
			}
		}

		public virtual bool CanSellItem(Item item, IShopSellInfo ssi)
		{
			if (item == null || item.Deleted || !item.Movable || !item.IsStandardLoot() || !ssi.IsSellable(item))
			{
				return false;
			}

			if (item is Container && item.Items.Count > 0)
			{
				return false;
			}

			var p = item.Parent as Item;

			while (p != null)
			{
				if (p is ILockable && ((ILockable)p).Locked)
				{
					return false;
				}

				p = p.Parent as Item;
			}

			return true;
		}

		public override bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
		{
			if (!Trading || !IsActiveSeller || CashType == null || !CashType.IsNotNull)
			{
				return false;
			}

			if (!buyer.CheckAlive())
			{
				return false;
			}

			if (!CheckVendorAccess(buyer))
			{
				Say("I can't serve you! Company policy.");
				//Say(501522); // I shall not treat with scum like thee!
				return false;
			}

			UpdateBuyInfo();

			var info = GetSellInfo();
			var totalCost = 0;
			var validBuy = new List<BuyItemResponse>(list.Count);
			var fromBank = false;
			var fullPurchase = true;
			var controlSlots = buyer.FollowersMax - buyer.Followers;

			foreach (var buy in list)
			{
				var ser = buy.Serial;
				var amount = buy.Amount;

				if (ser.IsItem)
				{
					var item = World.FindItem(ser);

					if (item == null)
					{
						continue;
					}

					var gbi = LookupDisplayObject(item);

					if (gbi != null)
					{
						ProcessSinglePurchase(buy, gbi, validBuy, ref controlSlots, ref fullPurchase, ref totalCost);
					}
					else if (item != BuyPack && item.IsChildOf(BuyPack))
					{
						if (amount > item.Amount)
						{
							amount = item.Amount;
						}

						if (amount <= 0)
						{
							continue;
						}

						foreach (var ssi in info.Where(ssi => ssi.IsSellable(item) && ssi.IsResellable(item)))
						{
							totalCost += ssi.GetBuyPriceFor(item) * amount;
							validBuy.Add(buy);
							break;
						}
					}
				}
				else if (ser.IsMobile)
				{
					var mob = World.FindMobile(ser);

					if (mob == null)
					{
						continue;
					}

					var gbi = LookupDisplayObject(mob);

					if (gbi != null)
					{
						ProcessSinglePurchase(buy, gbi, validBuy, ref controlSlots, ref fullPurchase, ref totalCost);
					}
				}
			}

			if (fullPurchase && validBuy.Count == 0)
			{
				// Thou hast bought nothing!
				SayTo(buyer, 500190);
			}
			else if (validBuy.Count == 0)
			{
				// Your order cannot be fulfilled, please try again.
				SayTo(buyer, 500187);
			}

			if (validBuy.Count == 0)
			{
				return false;
			}

			var bought = buyer.AccessLevel >= AccessLevel.GameMaster;

			if (!bought && CashType.TypeEquals<ObjectProperty>())
			{
				var cashSource = GetCashObject(buyer) ?? buyer;

				bought = CashProperty.Consume(cashSource, totalCost);

				if (!bought)
				{
					// Begging thy pardon, but thou cant afford that.
					SayTo(buyer, 500192);
					return false;
				}

				SayTo(buyer, "{0:#,0} {1} has been deducted from your total.", totalCost, CashName.GetString(buyer));
			}

			var isGold = CashType.TypeEquals<Gold>();

			var cont = buyer.Backpack;

			if (!bought && cont != null && isGold)
			{
				VitaNexCore.TryCatch(
					() =>
					{
						var lt = ScriptCompiler.FindTypeByName("GoldLedger") ?? Type.GetType("GoldLedger");

						if (lt == null)
						{
							return;
						}

						var ledger = cont.FindItemByType(lt);

						if (ledger == null || ledger.Deleted)
						{
							return;
						}

						var lp = lt.GetProperty("Gold");

						if (lp == null)
						{
							return;
						}

						if (lp.PropertyType.IsEqual<Int64>())
						{
							var lg = (long)lp.GetValue(ledger, null);

							if (lg < totalCost)
							{
								return;
							}

							lp.SetValue(ledger, lg - totalCost, null);
							bought = true;
						}
						else if (lp.PropertyType.IsEqual<Int32>())
						{
							var lg = (int)lp.GetValue(ledger, null);

							if (lg < totalCost)
							{
								return;
							}

							lp.SetValue(ledger, lg - totalCost, null);
							bought = true;
						}

						if (bought)
						{
							buyer.SendMessage(2125, "{0:#,0} gold has been withdrawn from your ledger.", totalCost);
						}
					});
			}

			if (!bought)
			{
				ConsumeCurrency(buyer, ref totalCost, ref bought);
			}

			if (!bought && cont != null)
			{
				if (cont.ConsumeTotal(CashType, totalCost))
				{
					bought = true;
				}
			}

			if (!bought && isGold && Banker.Withdraw(buyer, totalCost))
			{
				bought = true;
				fromBank = true;
			}

			if (!bought && !isGold)
			{
				cont = buyer.FindBankNoCreate();

				if (cont != null && cont.ConsumeTotal(CashType, totalCost))
				{
					bought = true;
					fromBank = true;
				}
			}

			if (!bought)
			{
				// Begging thy pardon, but thou cant afford that.
				SayTo(buyer, 500192);
				return false;
			}

			buyer.PlaySound(0x32);

			cont = buyer.Backpack ?? buyer.BankBox;

			foreach (var buy in validBuy)
			{
				var ser = buy.Serial;
				var amount = buy.Amount;

				if (amount < 1)
				{
					continue;
				}

				if (ser.IsItem)
				{
					var item = World.FindItem(ser);

					if (item == null)
					{
						continue;
					}

					var gbi = LookupDisplayObject(item);

					if (gbi != null)
					{
						ProcessValidPurchase(amount, gbi, buyer, cont);
					}
					else
					{
						if (amount > item.Amount)
						{
							amount = item.Amount;
						}

						if (info.Where(ssi => ssi.IsSellable(item)).Any(ssi => ssi.IsResellable(item)))
						{
							Item buyItem;

							if (amount >= item.Amount)
							{
								buyItem = item;
							}
							else
							{
								buyItem = LiftItemDupe(item, item.Amount - amount) ?? item;
							}

							if (cont == null || !cont.TryDropItem(buyer, buyItem, false))
							{
								buyItem.MoveToWorld(buyer.Location, buyer.Map);
							}
						}
					}
				}
				else if (ser.IsMobile)
				{
					var mob = World.FindMobile(ser);

					if (mob == null)
					{
						continue;
					}

					var gbi = LookupDisplayObject(mob);

					if (gbi != null)
					{
						ProcessValidPurchase(amount, gbi, buyer, cont);
					}
				}
			}

			if (fullPurchase)
			{
				if (buyer.AccessLevel >= AccessLevel.GameMaster)
				{
					SayTo(buyer, true, "I would not presume to charge thee anything.  Here are the goods you requested.");
				}
				else if (fromBank)
				{
					SayTo(
						buyer,
						true,
						"The total of thy purchase is {0:#,0} {1}, which has been withdrawn from your bank account.  My thanks for the patronage.",
						totalCost,
						CashName.GetString(buyer));
				}
				else
				{
					SayTo(
						buyer,
						true,
						"The total of thy purchase is {0:#,0} {1}.  My thanks for the patronage.",
						totalCost,
						CashName.GetString(buyer));
				}
			}
			else
			{
				if (buyer.AccessLevel >= AccessLevel.GameMaster)
				{
					SayTo(
						buyer,
						true,
						"I would not presume to charge thee anything.  Unfortunately, I could not sell you all the goods you requested.");
				}
				else if (fromBank)
				{
					SayTo(
						buyer,
						true,
						"The total of thy purchase is {0:#,0} {1}, which has been withdrawn from your bank account.  My thanks for the patronage.  Unfortunately, I could not sell you all the goods you requested.",
						totalCost,
						CashName.GetString(buyer));
				}
				else
				{
					SayTo(
						buyer,
						true,
						"The total of thy purchase is {0:#,0} {1}.  My thanks for the patronage.  Unfortunately, I could not sell you all the goods you requested.",
						totalCost,
						CashName.GetString(buyer));
				}
			}

			return true;
		}

		public virtual void ConsumeCurrency(Mobile buyer, ref int amount, ref bool bought)
		{ }

		public override bool OnSellItems(Mobile seller, List<SellItemResponse> list)
		{
			if (!Trading || CashType == null || !CashType.IsNotNull)
			{
				return false;
			}

			if (!IsActiveBuyer)
			{
				return false;
			}

			if (!seller.CheckAlive())
			{
				return false;
			}

			if (!CheckVendorAccess(seller))
			{
				Say("I can't serve you! Company policy.");
				//Say(501522); // I shall not treat with scum like thee!
				return false;
			}

			seller.PlaySound(0x32);

			var info = GetSellInfo();
			var buyInfo = GetBuyInfo();
			var giveCurrency = 0;
			Container cont;

			var finalList = list.Where(resp => CanSell(seller, info, resp)).ToArray();

			if (finalList.Length > 500)
			{
				SayTo(seller, true, "You may only sell 500 items at a time!");
				return false;
			}

			if (finalList.Length == 0)
			{
				return true;
			}

			foreach (var resp in finalList)
			{
				foreach (var ssi in info)
				{
					if (!ssi.IsSellable(resp.Item))
					{
						continue;
					}

					var amount = resp.Amount;

					if (amount > resp.Item.Amount)
					{
						amount = resp.Item.Amount;
					}

					var worth = GetSellPrice(seller, ssi, resp) * amount;

					if (ssi.IsResellable(resp.Item))
					{
						var found = false;

						if (buyInfo.Any(bii => bii.Restock(resp.Item, amount)))
						{
							resp.Item.Consume(amount);
							found = true;
						}

						if (!found)
						{
							cont = BuyPack;

							if (amount < resp.Item.Amount)
							{
								var item = LiftItemDupe(resp.Item, resp.Item.Amount - amount);

								if (item != null)
								{
									item.SetLastMoved();

									if (!cont.TryDropItem(this, resp.Item, false))
									{
										resp.Item.Delete();
									}
								}
								else
								{
									resp.Item.SetLastMoved();

									if (!cont.TryDropItem(this, resp.Item, false))
									{
										resp.Item.Delete();
									}
								}
							}
							else
							{
								resp.Item.SetLastMoved();

								if (!cont.TryDropItem(this, resp.Item, false))
								{
									resp.Item.Delete();
								}
							}
						}
					}
					else
					{
						if (amount < resp.Item.Amount)
						{
							resp.Item.Amount -= amount;
						}
						else
						{
							resp.Item.Delete();
						}
					}

					giveCurrency += worth;
					break;
				}
			}

			if (giveCurrency <= 0)
			{
				return false;
			}

			while (giveCurrency > 0)
			{
				Item c = null;

				if (CashType.TypeEquals<ObjectProperty>())
				{
					var cashSource = GetCashObject(seller) ?? seller;

					if (!CashProperty.Add(cashSource, giveCurrency))
					{
						c = new Static(0x14F0, 1)
						{
							Name = String.Format("Staff I.O.U [{0} {1}]", giveCurrency.ToString("#,0"), CashName.GetString(seller)),
							Hue = 85,
							Movable = true,
							BlessedFor = seller,
							LootType = LootType.Blessed
						};
					}

					giveCurrency = 0;
				}

				if (c == null && giveCurrency > 0)
				{
					c = CashType.CreateInstance<Item>();

					if (c == null)
					{
						c = new Static(0x14F0, 1)
						{
							Name = String.Format("Staff I.O.U [{0} {1}]", giveCurrency.ToString("#,0"), CashName.GetString(seller)),
							Hue = 85,
							Movable = true,
							BlessedFor = seller,
							LootType = LootType.Blessed
						};
						giveCurrency = 0;
					}
					else
					{
						if (giveCurrency >= 60000)
						{
							c.Amount = 60000;
							giveCurrency -= 60000;
						}
						else
						{
							c.Amount = giveCurrency;
							giveCurrency = 0;
						}
					}
				}

				c.GiveTo(seller);
			}

			seller.PlaySound(0x0037); //Gold dropping sound
			
			return true;
		}

		public virtual bool CanSell(Mobile seller, IShopSellInfo[] info, SellItemResponse resp)
		{
			return resp.Item != null && resp.Item.RootParent == seller && resp.Amount > 0 && resp.Item.Movable &&
				   (!(resp.Item is Container) || resp.Item.Items.Count == 0) && info.Any(ssi => ssi.IsSellable(resp.Item));
		}

		public virtual int GetSellPrice(Mobile seller, IShopSellInfo info, SellItemResponse resp)
		{
			return info.GetSellPriceFor(resp.Item);
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			var eopl = new ExtendedOPL(list);

			if (!Trading)
			{
				eopl.Add("Not Trading".WrapUOHtmlColor(Color.OrangeRed));
				eopl.Apply();
				return;
			}

			if (!ShowCashName)
			{
				return;
			}

			var name = CashName.GetString();

			if (!String.IsNullOrWhiteSpace(name))
			{
				eopl.Add("Trades For {0}".WrapUOHtmlColor(Color.SkyBlue), name);
			}

			var races = RequiredRaces;

			if (races != null && races.Length > 0)
			{
				foreach (var r in races)
				{
					eopl.Add("Trades With {0}".WrapUOHtmlColor(Color.LawnGreen), r.PluralName);
				}
			}

			eopl.Apply();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			var version = writer.SetVersion(2);

			switch (version)
			{
				case 2:
					writer.WriteTextDef(_CashAbbr);
					goto case 1;
				case 1:
					CashProperty.Serialize(writer);
					goto case 0;
				case 0:
				{
					CashType.Serialize(writer);

					writer.WriteTextDef(_CashName);
					writer.Write(_ShowCashName);

					writer.Write(_Trading);

					writer.Write(Discount);
					writer.Write(DiscountEnabled);
					writer.Write(DiscountYell);
				}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.GetVersion();

			switch (version)
			{
				case 2:
					_CashAbbr = reader.ReadTextDef();
					goto case 1;
				case 1:
					CashProperty = new ObjectProperty(reader);
					goto case 0;
				case 0:
				{
					if (version < 1)
					{
						var t = new ItemTypeSelectProperty(reader);
						CashType = t.InternalType;
					}
					else
					{
						CashType = new TypeSelectProperty<object>(reader);
					}

					_CashName = reader.ReadTextDef();
					_ShowCashName = reader.ReadBool();

					_Trading = reader.ReadBool();

					Discount = reader.ReadInt();
					DiscountEnabled = reader.ReadBool();
					DiscountYell = reader.ReadBool();
				}
					break;
			}

			if (CashProperty == null)
			{
				CashProperty = new ObjectProperty();
			}

			if (version < 2)
			{
				_CashAbbr = String.Join(String.Empty, _CashName.GetString().Select(Char.IsUpper));
			}
		}
	}
}