#region Header
//   Vorspire    _,-'/-'/  Statistics.cs
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

using VitaNex.IO;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	[PropertyObject]
	public sealed class DonationStatistics
	{
		[CommandProperty(AutoDonate.Access)]
		public DataStoreStatus ProfileStatus { get { return AutoDonate.Profiles.Status; } }

		[CommandProperty(AutoDonate.Access)]
		public int ProfileCount { get { return AutoDonate.Profiles.Count; } }

		[CommandProperty(AutoDonate.Access)]
		public double TotalIncome
		{
			get { return ProfileCount <= 0 ? 0 : AutoDonate.Profiles.Values.Sum(p => p.TotalValue); }
		}

		[CommandProperty(AutoDonate.Access)]
		public long TotalCredit { get { return ProfileCount <= 0 ? 0 : AutoDonate.Profiles.Values.Sum(p => p.TotalCredit); } }

		[CommandProperty(AutoDonate.Access)]
		public int TierMin { get { return ProfileCount <= 0 ? 0 : AutoDonate.Profiles.Values.Min(p => p.Tier); } }

		[CommandProperty(AutoDonate.Access)]
		public int TierMax { get { return ProfileCount <= 0 ? 0 : AutoDonate.Profiles.Values.Max(p => p.Tier); } }

		[CommandProperty(AutoDonate.Access)]
		public double TierAverage { get { return ProfileCount <= 0 ? 0 : AutoDonate.Profiles.Values.Average(p => p.Tier); } }

		[CommandProperty(AutoDonate.Access)]
		public DataStoreStatus TransStatus { get { return AutoDonate.Transactions.Status; } }

		[CommandProperty(AutoDonate.Access)]
		public int TransCount { get { return AutoDonate.Transactions.Count; } }

		[CommandProperty(AutoDonate.Access)]
		public double TransMin { get { return TransCount <= 0 ? 0 : AutoDonate.Transactions.Values.Min(p => p.Total); } }

		[CommandProperty(AutoDonate.Access)]
		public double TransMax { get { return TransCount <= 0 ? 0 : AutoDonate.Transactions.Values.Max(p => p.Total); } }

		[CommandProperty(AutoDonate.Access)]
		public double TransAverage
		{
			get { return TransCount <= 0 ? 0 : AutoDonate.Transactions.Values.Average(t => t.Total); }
		}

		[CommandProperty(AutoDonate.Access)]
		public long CreditMin { get { return TransCount <= 0 ? 0 : AutoDonate.Transactions.Values.Min(p => p.Credit); } }

		[CommandProperty(AutoDonate.Access)]
		public long CreditMax { get { return TransCount <= 0 ? 0 : AutoDonate.Transactions.Values.Max(p => p.Credit); } }

		[CommandProperty(AutoDonate.Access)]
		public double CreditAverage
		{
			get { return TransCount <= 0 ? 0 : AutoDonate.Transactions.Values.Average(t => t.Credit); }
		}

		public override string ToString()
		{
			return "Donation Statistics";
		}
	}
}