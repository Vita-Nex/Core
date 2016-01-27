#region Header
//   Vorspire    _,-'/-'/  SystemOpts.cs
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

using ReqFlags = VitaNex.Modules.WebStats.WebStatsRequestFlags;
#endregion

namespace VitaNex.Modules.WebStats
{
	public class WebStatsOptions : CoreModuleOptions
	{
		[CommandProperty(WebStats.Access)]
		public TimeSpan UpdateInterval { get; set; }

		[CommandProperty(WebStats.Access, true)]
		public ReqFlags RequestFlags { get; private set; }

		[CommandProperty(WebStats.Access)]
		public bool DisplayServer
		{
			get { return GetRequestFlag(ReqFlags.Server); }
			set { SetRequestFlag(ReqFlags.Server, value); }
		}

		[CommandProperty(WebStats.Access)]
		public bool DisplayStats
		{
			get { return GetRequestFlag(ReqFlags.Stats); }
			set { SetRequestFlag(ReqFlags.Stats, value); }
		}

		[CommandProperty(WebStats.Access)]
		public bool DisplayPlayers
		{
			get { return GetRequestFlag(ReqFlags.Players); }
			set { SetRequestFlag(ReqFlags.Players, value); }
		}

		[CommandProperty(WebStats.Access)]
		public bool DisplayPlayerGuilds
		{
			get { return GetRequestFlag(ReqFlags.PlayerGuilds); }
			set { SetRequestFlag(ReqFlags.PlayerGuilds, value); }
		}

		[CommandProperty(WebStats.Access)]
		public bool DisplayPlayerStats
		{
			get { return GetRequestFlag(ReqFlags.PlayerStats); }
			set { SetRequestFlag(ReqFlags.PlayerStats, value); }
		}

		[CommandProperty(WebStats.Access)]
		public bool DisplayPlayerSkills
		{
			get { return GetRequestFlag(ReqFlags.PlayerSkills); }
			set { SetRequestFlag(ReqFlags.PlayerSkills, value); }
		}

		[CommandProperty(WebStats.Access)]
		public bool DisplayPlayerEquip
		{
			get { return GetRequestFlag(ReqFlags.PlayerEquip); }
			set { SetRequestFlag(ReqFlags.PlayerEquip, value); }
		}

		private bool GetRequestFlag(ReqFlags flag)
		{
			return RequestFlags.HasFlag(flag);
		}

		private void SetRequestFlag(ReqFlags flag, bool value)
		{
			if (flag == ReqFlags.None)
			{
				RequestFlags = value ? flag : ReqFlags.All;
				return;
			}

			if (flag == ReqFlags.All)
			{
				RequestFlags = value ? flag : ReqFlags.None;
				return;
			}

			if (value && !flag.HasFlag(ReqFlags.Players) &&
				flag.AnyFlags(ReqFlags.PlayerGuilds, ReqFlags.PlayerStats, ReqFlags.PlayerSkills, ReqFlags.PlayerEquip))
			{
				flag |= ReqFlags.Players;
			}
			else if (!value && flag.HasFlag(ReqFlags.Players))
			{
				flag |= ReqFlags.PlayerGuilds | ReqFlags.PlayerStats | ReqFlags.PlayerSkills | ReqFlags.PlayerEquip;
			}

			if (value)
			{
				RequestFlags |= flag;
			}
			else
			{
				RequestFlags &= ~flag;
			}
		}

		public WebStatsOptions()
			: base(typeof(WebStats))
		{
			RequestFlags = ReqFlags.Server | ReqFlags.Stats;
		}

		public WebStatsOptions(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			base.Clear();

			UpdateInterval = TimeSpan.Zero;
			RequestFlags = ReqFlags.None;
		}

		public override void Reset()
		{
			base.Reset();

			UpdateInterval = TimeSpan.FromSeconds(30.0);
			RequestFlags = ReqFlags.Server | ReqFlags.Stats;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			var version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
				case 0:
				{
					writer.Write(UpdateInterval);
					writer.WriteFlag(RequestFlags);
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
				case 1:
				case 0:
				{
					if (version < 1)
					{
						reader.ReadShort();
						reader.ReadInt();
					}

					UpdateInterval = reader.ReadTimeSpan();
					RequestFlags = reader.ReadFlag<ReqFlags>();
				}
					break;
			}
		}
	}
}