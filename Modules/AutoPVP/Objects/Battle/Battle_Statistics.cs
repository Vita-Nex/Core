#region Header
//   Vorspire    _,-'/-'/  Battle_Statistics.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;

using Server;
using Server.Mobiles;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public abstract partial class PvPBattle
	{
		[CommandProperty(AutoPvP.Access)]
		public virtual bool Ranked { get; set; }

		public Dictionary<PlayerMobile, PvPProfileHistoryEntry> Statistics { get; private set; }
		public Dictionary<PlayerMobile, PvPProfileHistoryEntry> StatisticsCache { get; private set; }

		public PvPProfileHistoryEntry EnsureStatistics(PlayerMobile pm)
		{
			return EnsureStatistics(pm, false);
		}

		public PvPProfileHistoryEntry EnsureStatistics(PlayerMobile pm, bool replace)
		{
			PvPProfileHistoryEntry entry;

			if (!Statistics.TryGetValue(pm, out entry) || entry == null || replace)
			{
				Statistics[pm] = entry = new PvPProfileHistoryEntry(AutoPvP.CurrentSeason.Number);
			}

			StatisticsCache[pm] = entry;

			return entry;
		}

		public void TransferStatistics()
		{
			if (!Ranked)
			{
				Statistics.Clear();
				return;
			}

			foreach (var pm in Statistics.Keys)
			{
				TransferStatistics(pm);
			}

			OnTransferStatistics();

			Statistics.Clear();
		}

		public void TransferStatistics(PlayerMobile pm)
		{
			if (!Ranked)
			{
				Statistics.Remove(pm);
				return;
			}

			var profile = AutoPvP.EnsureProfile(pm);
			var entry = EnsureStatistics(pm);

			OnTransferStatistics(pm, profile.Statistics, entry);

			Statistics.Remove(pm);
		}

		protected virtual void OnTransferStatistics()
		{ }

		protected virtual void OnTransferStatistics(
			PlayerMobile pm,
			PvPProfileHistoryEntry profileStats,
			PvPProfileHistoryEntry battleStats)
		{
			if (!Ranked || profileStats == null || battleStats == null)
			{
				return;
			}

			profileStats.Battles += battleStats.Battles;
			profileStats.Wins += battleStats.Wins;
			profileStats.Losses += battleStats.Losses;
			profileStats.Kills += battleStats.Kills;
			profileStats.Deaths += battleStats.Deaths;
			profileStats.Resurrections += battleStats.Resurrections;
			profileStats.DamageTaken += battleStats.DamageTaken;
			profileStats.DamageDone += battleStats.DamageDone;
			profileStats.HealingTaken += battleStats.HealingTaken;
			profileStats.HealingDone += battleStats.HealingDone;

			foreach (var kvp in battleStats.MiscStats)
			{
				profileStats[kvp.Key] += kvp.Value;
			}
		}
	}
}