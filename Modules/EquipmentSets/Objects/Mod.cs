#region Header
//   Vorspire    _,-'/-'/  Mod.cs
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

using VitaNex.Network;
#endregion

namespace VitaNex.Modules.EquipmentSets
{
	public abstract class EquipmentSetMod
	{
		public List<Mobile> ActiveOwners { get; private set; }

		public string Name { get; set; }
		public string Desc { get; set; }
		public int PartsRequired { get; set; }
		public bool Display { get; set; }

		public EquipmentSetMod(string name = "Set Mod", string desc = null, int partsReq = 1, bool display = true)
		{
			Name = name;
			Desc = desc ?? String.Empty;
			Display = display;
			PartsRequired = partsReq;

			ActiveOwners = new List<Mobile>();
		}

		public bool IsActive(Mobile m)
		{
			return m != null && ActiveOwners.Contains(m);
		}

		public bool Activate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped)
		{
			if (m == null || m.Deleted || equipped == null)
			{
				return false;
			}

			if (OnActivate(m, equipped))
			{
				if (!ActiveOwners.Contains(m))
				{
					ActiveOwners.Add(m);
				}

				return true;
			}

			return false;
		}

		public bool Deactivate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped)
		{
			if (m == null || m.Deleted || equipped == null)
			{
				return false;
			}

			return ActiveOwners.Remove(m) && OnDeactivate(m, equipped);
		}

		protected abstract bool OnActivate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped);
		protected abstract bool OnDeactivate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped);

		public virtual void GetProperties(Mobile viewer, ExtendedOPL list, bool equipped)
		{
			if (!String.IsNullOrEmpty(Desc))
			{
				list.Add(
					"[{0:#,0}] {1}: {2}".WrapUOHtmlColor(
						equipped && IsActive(viewer) ? EquipmentSets.CMOptions.ModNameColorRaw : EquipmentSets.CMOptions.InactiveColorRaw),
					PartsRequired,
					Name.ToUpperWords(),
					Desc);
			}
			else
			{
				list.Add(
					"[{0:#,0}] {1}".WrapUOHtmlColor(
						equipped && IsActive(viewer) ? EquipmentSets.CMOptions.ModNameColorRaw : EquipmentSets.CMOptions.InactiveColorRaw),
					PartsRequired,
					Name.ToUpperWords());
			}
		}

		public override string ToString()
		{
			return String.Format("{0}", Name);
		}
	}
}