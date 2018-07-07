#region Header
//   Vorspire    _,-'/-'/  TournamentArchives.cs
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
using System.IO;
using System.Linq;

using Server;
using Server.Mobiles;

using VitaNex.IO;
#endregion

namespace VitaNex.Modules.AutoPvP.Battles
{
	public static class TournamentArchives
	{
		public static TournamentArchive TestArchive { get; private set; }

		static TournamentArchives()
		{
			Registry = new List<TournamentArchive>();

			TestArchive = new TournamentArchive("Test", DateTime.UtcNow);
		}

		public static void Initialize()
		{
			var start = DateTime.UtcNow - TimeSpan.FromDays(1.0);

			var players = World.Mobiles.OfType<PlayerMobile>().ToQueue();

			var index = 0;

			while (players.Count > 1)
			{
				var r = new TournamentMatch(index++, 0, TimeSpan.Zero, TimeSpan.FromMinutes(1.0));

				players.DequeueRange(r.Players);

				r.DateStart = start;
				r.DateEnd = start = start.Add(r.Delay + r.Duration);

				for (var i = 1; i <= 3; i++)
				{
					r.Record("Testing {0}...", i);
				}

				r.ForEachPlayer(
					p =>
					{
						var count = Utility.RandomMinMax(1, 3);

						while (--count >= 0)
						{
							r.RecordDamage(p, Utility.RandomMinMax(100, 1000));
							r.RecordHeal(p, Utility.RandomMinMax(100, 1000));
						}
					});

				r.Winner = r.Players.Highest(r.ComputeScore);

				TestArchive.Matches.Add(r);

				players.Enqueue(r.Winner);
			}

			CommandUtility.Register("TournamentArchives", AutoPvP.Access, e => new TournamentArchivesUI(e.Mobile).Send());
			CommandUtility.RegisterAlias("TournamentArchives", "TArchives");

			CommandUtility.Register("TestArcUI", AutoPvP.Access, e => new TournamentArchiveUI(e.Mobile, TestArchive).Send());
		}

		public static FileInfo File
		{
			get { return IOUtility.EnsureFile(VitaNexCore.SavesDirectory + "/AutoPvP/Tournament/Archives.bin"); }
		}

		public static List<TournamentArchive> Registry { get; private set; }

		public static int Count { get { return Registry.Count; } }

		/*static TournamentArchives()
		{
			Registry = new List<TournamentArchive>();
		}*/

		public static void Configure()
		{
			VitaNexCore.OnModuleLoaded += OnModuleLoaded;
			VitaNexCore.OnModuleSaved += OnModuleSaved;
		}

		private static void OnModuleLoaded(CoreModuleInfo cmi)
		{
			if (cmi.Enabled && cmi.TypeOf == typeof(AutoPvP))
			{
				File.Deserialize(r => r.ReadBlockList(r1 => new TournamentArchive(r1), Registry));
			}
		}

		private static void OnModuleSaved(CoreModuleInfo cmi)
		{
			if (cmi.Enabled && cmi.TypeOf == typeof(AutoPvP))
			{
				File.Serialize(w => w.WriteBlockList(Registry, (w1, o) => o.Serialize(w1)));
			}
		}

		public static TournamentArchive CreateArchive(TournamentBattle b)
		{
			TournamentArchive a = null;

			foreach (var r in b.Matches.Where(o => !o.IsDisposed && o.IsComplete && o.Records.Count > 0))
			{
				if (a == null)
				{
					a = new TournamentArchive(b.Name, DateTime.UtcNow);
				}

				a.Matches.Add(r);
			}

			if (a != null)
			{
				Registry.Add(a);
			}

			return a;
		}
	}
}