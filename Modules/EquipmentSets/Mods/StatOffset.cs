#region Header
//   Vorspire    _,-'/-'/  StatOffset.cs
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

using Server;
#endregion

namespace VitaNex.Modules.EquipmentSets
{
	public class StatOffsetSetMod : EquipmentSetMod
	{
		public string UID { get; private set; }

		private StatType _Stat;

		public StatType Stat
		{
			get { return _Stat; }
			set
			{
				_Stat = value;
				InvalidateDesc();
			}
		}

		private int _Offset;

		public int Offset
		{
			get { return _Offset; }
			set
			{
				_Offset = value;
				InvalidateDesc();
			}
		}

		private TimeSpan _Duration;

		public TimeSpan Duration
		{
			get { return _Duration; }
			set
			{
				_Duration = value;
				InvalidateDesc();
			}
		}

		public StatOffsetSetMod(
			string uid = null,
			string name = "Stat Mod",
			int partsReq = 1,
			bool display = true,
			StatType stat = StatType.All,
			int offset = 1,
			TimeSpan? duration = null)
			: base(name, null, partsReq, display)
		{
			UID = uid ?? Name + TimeStamp.UtcNow;

			_Stat = stat;
			_Offset = offset;
			_Duration = duration ?? TimeSpan.Zero;

			InvalidateDesc();
		}

		public virtual void InvalidateDesc()
		{
			var statName = String.Empty;

			switch (_Stat)
			{
				case StatType.All:
					statName = "All Stats";
					break;
				case StatType.Dex:
					statName = "Dexterity";
					break;
				case StatType.Int:
					statName = "Intelligence";
					break;
				case StatType.Str:
					statName = "Strength";
					break;
			}

			Desc = _Duration > TimeSpan.Zero
				? String.Format("Increases {0} By {1:#,0} for {2}", statName, _Offset, _Duration.ToSimpleString("h:m:s"))
				: String.Format("Increases {0} By {1}", statName, _Offset);
		}

		protected override bool OnActivate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped)
		{
			if (m == null || m.Deleted || equipped == null)
			{
				return false;
			}

			if (EquipmentSets.CMOptions.ModuleDebug)
			{
				EquipmentSets.CMOptions.ToConsole(
					"OnActivate: '{0}', '{1}', '{2}', '{3}', '{4}'",
					m,
					UID,
					_Stat,
					_Offset,
					_Duration.ToSimpleString());
			}

			m.AddStatMod(new StatMod(_Stat, UID, _Offset, _Duration));
			return true;
		}

		protected override bool OnDeactivate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped)
		{
			if (m == null || m.Deleted || equipped == null)
			{
				return false;
			}

			if (EquipmentSets.CMOptions.ModuleDebug)
			{
				EquipmentSets.CMOptions.ToConsole(
					"OnDeactivate: '{0}', '{1}', '{2}', '{3}', '{4}'",
					m,
					UID,
					_Stat,
					_Offset,
					_Duration.ToSimpleString());
			}

			m.RemoveStatMod(UID);

			return true;
		}
	}
}