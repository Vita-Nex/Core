#region Header
//   Vorspire    _,-'/-'/  SuperCraft.cs
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

using Server;
using Server.Engines.Craft;
using Server.Items;
#endregion

namespace VitaNex.SuperCrafts
{
	public abstract class SuperCraftSystem : CraftSystem
	{
		public static List<SuperCraftSystem> Instances { get; private set; }

		static SuperCraftSystem()
		{
			var types = typeof(SuperCraftSystem).GetConstructableChildren();

			Instances = types.Select(t => t.CreateInstanceSafe<SuperCraftSystem>()).Where(cs => cs != null).ToList();

			var sys = new ObjectProperty("Systems");

			var list = sys.GetValue(typeof(CraftSystem)) as List<CraftSystem>;

			if (list != null)
			{
				list.AddRange(Instances);
				list.Prune();
			}
		}

		// If there are issues with initialization, uncomment the next line:
		//[CallPriority(Int32.MaxValue - (Int16.MaxValue * 2))]
		public static void Configure()
		{
			Instances.Prune();
		}

		public static SuperCraftSystem Resolve(Type tSys)
		{
			return Instances.FirstOrDefault(cs => cs.TypeEquals(tSys));
		}

		public static TSys Resolve<TSys>()
			where TSys : SuperCraftSystem
		{
			return Instances.OfType<TSys>().FirstOrDefault();
		}

		public abstract TextDefinition GumpTitle { get; }

		public sealed override int GumpTitleNumber { get { return GumpTitle.Number; } }
		public sealed override string GumpTitleString { get { return GumpTitle.String; } }

		public SuperCraftSystem(int minCraftEffect, int maxCraftEffect, double delay)
			: base(minCraftEffect, maxCraftEffect, delay)
		{ }

		public abstract override void InitCraftList();

#if ServUO
		public override int CanCraft(Mobile m, ITool tool, Type itemType)
#else
		public override int CanCraft(Mobile m, BaseTool tool, Type itemType)
#endif
		{
			if (tool == null || tool.Deleted || tool.UsesRemaining < 0)
			{
				return 1044038; // You have worn out your tool!
			}

			if (tool is Item && !BaseTool.CheckAccessible((Item)tool, m))
			{
				return 1044263; // The tool must be on your person to use.
			}

			return 0;
		}

		public virtual int AddCraft<TItem>(
			TextDefinition group,
			TextDefinition name,
			double skillReq,
			ResourceInfo[] resources,
			Action<CraftItem> onAdded = null)
			where TItem : Item
		{
			return AddCraft<TItem>(group, name, MainSkill, skillReq, resources, onAdded);
		}

		public virtual int AddCraft<TItem>(
			TextDefinition group,
			TextDefinition name,
			SkillName skill,
			double skillReq,
			ResourceInfo[] resources,
			Action<CraftItem> onAdded = null)
			where TItem : Item
		{
			return AddCraft(new CraftInfo(typeof(TItem), group, name, skill, skillReq, resources, onAdded));
		}

		public virtual int AddCraft<TItem>(
			TextDefinition group,
			TextDefinition name,
			double minSkill,
			double maxSkill,
			ResourceInfo[] resources,
			Action<CraftItem> onAdded = null)
			where TItem : Item
		{
			return AddCraft<TItem>(group, name, MainSkill, minSkill, maxSkill, resources, onAdded);
		}

		public virtual int AddCraft<TItem>(
			TextDefinition group,
			TextDefinition name,
			SkillName skill,
			double minSkill,
			double maxSkill,
			ResourceInfo[] resources,
			Action<CraftItem> onAdded = null)
			where TItem : Item
		{
			return AddCraft(new CraftInfo(typeof(TItem), group, name, skill, minSkill, maxSkill, resources, onAdded));
		}

		public virtual int AddCraft(CraftInfo info)
		{
			if (info == null || info.TypeOf == null || info.Resources == null || info.Resources.Length == 0)
			{
				return -1;
			}

			if (!info.TypeOf.IsConstructableFrom<Item>())
			{
				return -1;
			}

			var res = info.Resources[0];

			var msg = String.Format("You do not have the required {0} to craft that item.", res.Name.GetString());

			var index = AddCraft(
				info.TypeOf,
				info.Group,
				info.Name,
				info.Skill,
				info.MinSkill,
				info.MaxSkill,
				res.TypeOf,
				res.Name,
				res.Amount,
				msg);

			if (info.Resources.Length > 1)
			{
				foreach (var r in info.Resources.Skip(1))
				{
					AddRes(
						index,
						r.TypeOf,
						r.Name,
						r.Amount,
						String.Format("You do not have the required {0} to craft that item.", r.Name.GetString()));
				}
			}

			info.OnAdded(CraftItems.GetAt(index));

			return index;
		}

		public CraftItem FindCraft<T>()
		{
			return FindCraft(typeof(T));
		}

		public virtual CraftItem FindCraft(Type type)
		{
			return FindCraft(type, true);
		}

		public CraftItem FindCraft<T>(bool subClasses)
		{
			return FindCraft(typeof(T), subClasses);
		}

		public virtual CraftItem FindCraft(Type type, bool subClasses)
		{
			if (CraftItems != null)
			{
				return subClasses ? CraftItems.SearchForSubclass(type) : CraftItems.SearchFor(type);
			}

			return null;
		}
	}
}