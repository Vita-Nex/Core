#region Header
//   Vorspire    _,-'/-'/  TimeBoostHours.cs
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
#endregion

namespace VitaNex.TimeBoosts
{
	public struct TimeBoostHours : ITimeBoost
	{
		public int RawValue { get; private set; }
		public TimeSpan Value { get; private set; }

		public string Desc { get { return "Hour"; } }
		public string Name { get { return String.Format("{0}-{1} Boost", RawValue, Desc); } }

		public int Hue { get { return 2118; } }

		public TimeBoostHours(int hours)
			: this()
		{
			Value = TimeSpan.FromHours(RawValue = hours);
		}

		public override string ToString()
		{
			return Name;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = RawValue;
				hash = (hash * 397) ^ Value.Days;
				hash = (hash * 397) ^ Value.Hours;
				hash = (hash * 397) ^ Value.Minutes;
				return hash;
			}
		}

		public static implicit operator TimeBoostHours(int hours)
		{
			return new TimeBoostHours(hours);
		}

		public static implicit operator TimeBoostHours(TimeSpan time)
		{
			return new TimeBoostHours(time.Hours);
		}

		public static implicit operator TimeSpan(TimeBoostHours value)
		{
			return value.Value;
		}

		public static implicit operator int(TimeBoostHours value)
		{
			return value.RawValue;
		}
	}
}