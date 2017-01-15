#region Header
//   Vorspire    _,-'/-'/  ProfileList.cs
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
using System.Globalization;
using System.Linq;

using Server;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPProfileListGump : ListGump<PvPProfile>
	{
		public PvPSeason Season { get; set; }

		public PvPProfileRankOrder RankSortOrder { get; set; }

		public bool UseConfirmDialog { get; set; }

		public PvPProfileListGump(
			Mobile user,
			PvPSeason season,
			Gump parent = null,
			bool useConfirm = true,
			PvPProfileRankOrder sortOrder = PvPProfileRankOrder.None)
			: base(user, parent, emptyText: "There are no profiles to display.", title: "PvP Profiles")
		{
			Season = season ?? AutoPvP.CurrentSeason;

			RankSortOrder = sortOrder != PvPProfileRankOrder.None ? sortOrder : AutoPvP.CMOptions.Advanced.Profiles.RankingOrder;

			UseConfirmDialog = useConfirm;

			CanSearch = User.AccessLevel > AccessLevel.Player || AutoPvP.CMOptions.Advanced.Profiles.AllowPlayerSearch;

			ForceRecompile = true;
		}

		protected override void Compile()
		{
			if (AutoPvP.SeasonSchedule.Enabled)
			{
				Title = "PvP Profiles (Season " + (Season != null ? Season.Number : AutoPvP.CurrentSeason.Number) + ")";
			}

			base.Compile();
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			list.Clear();

			if (User.AccessLevel >= AutoPvP.Access)
			{
				list.AppendEntry(
					new ListGumpEntry(
						"Delete All",
						button => new ConfirmDialogGump(User, this)
						{
							Title = "Delete All Profiles?",
							Html =
								"All profiles in the database will be deleted, erasing all data associated with them.\n" +
								"This action can not be reversed.\n\nDo you want to continue?",
							AcceptHandler = subButton =>
							{
								var profiles = new List<PvPProfile>(AutoPvP.Profiles.Values);

								foreach (var p in profiles.Where(p => p != null && !p.Deleted))
								{
									p.Delete();
								}

								Refresh(true);
							},
							CancelHandler = Refresh
						}.Send(),
						HighlightHue));
			}

			list.AppendEntry(new ListGumpEntry("My Profile", OnMyProfile));

			list.AppendEntry(
				new ListGumpEntry("Sort By (" + RankSortOrder + ")", b => new PvPProfileListSortGump(User, this, this, b).Send()));

			if (Season != null)
			{
				list.AppendEntry(
					new ListGumpEntry(
						"Overall Ranks",
						b =>
						{
							Season = null;
							Refresh(true);
						}));
			}

			var season = AutoPvP.CurrentSeason;

			if (Season != season)
			{
				list.AppendEntry(
					new ListGumpEntry(
						String.Format("Season {0:#,0} Ranks", season.Number),
						b =>
						{
							Season = season;
							Refresh(true);
						}));
			}

			if (season.Number > 1)
			{
				list.AppendEntry(
					new ListGumpEntry(
						"Select Season",
						b => new InputDialogGump(User, this)
						{
							Title = "Select Season",
							Html = "Enter the number for the season you wish to view rankings for.\nSeasons 1 to " + season.Number,
							InputText = Season == null ? "" : Season.Number.ToString(CultureInfo.InvariantCulture),
							Callback = (ib, text) =>
							{
								int num;

								if (Int32.TryParse(text, out num))
								{
									PvPSeason s;

									if (!AutoPvP.Seasons.TryGetValue(num, out s) || s == null)
									{
										User.SendMessage(ErrorHue, "Invalid Season selection.");
									}
									else
									{
										Season = s;
									}
								}

								Refresh(true);
							},
							CancelHandler = Refresh
						}.Send()));
			}

			base.CompileMenuOptions(list);
		}

		protected virtual void OnMyProfile(GumpButton button)
		{
			new PvPProfileOverviewGump(User, AutoPvP.EnsureProfile(User as PlayerMobile), Hide(true)).Send();
		}

		protected override void CompileList(List<PvPProfile> list)
		{
			list.Clear();
			list.AddRange(AutoPvP.GetSortedProfiles(RankSortOrder, Season));

			base.CompileList(list);
		}

		public override string GetSearchKeyFor(PvPProfile key)
		{
			if (key != null && !key.Deleted && key.Owner != null)
			{
				return key.Owner.RawName;
			}

			return base.GetSearchKeyFor(key);
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Add(
				"button/header/war",
				() => AddButton(
					85,
					15,
					2026,
					2025,
					b =>
					{
						if (Parent is PvPBattleListGump)
						{
							((PvPBattleListGump)Parent).Refresh(true);
						}
						else if (Parent is PvPBattleCategoryListGump)
						{
							((PvPBattleCategoryListGump)Parent).Refresh(true);
						}
						else if (!AutoPvP.CMOptions.Advanced.Misc.UseCategories)
						{
							new PvPBattleListGump(User, Hide(true), "All", UseConfirmDialog).Send();
						}
						else
						{
							new PvPBattleCategoryListGump(User, Hide(true), UseConfirmDialog).Send();
						}
					}));

			layout.Replace(
				"label/header/title",
				() => AddLabelCropped(160, 15, 215, 20, GetTitleHue(), String.IsNullOrEmpty(Title) ? DefaultTitle : Title));
		}

		protected override void CompileEntryLayout(SuperGumpLayout layout, Dictionary<int, PvPProfile> range)
		{
			layout.Add(
				"background/footer/help",
				() =>
				{
					AddBackground(0, 75 + (range.Count * 30), 420, 130, 9270);
					AddImageTiled(10, 85 + (range.Count * 30), 400, 110, 2624);
				});

			layout.Add("html/footer/help", () => AddHtml(20, 85 + (range.Count * 30), 390, 110, GetHelpText(), false, false));

			base.CompileEntryLayout(layout, range);
		}

		protected override void CompileEntryLayout(
			SuperGumpLayout layout,
			int length,
			int index,
			int pIndex,
			int yOffset,
			PvPProfile entry)
		{
			base.CompileEntryLayout(layout, length, index, pIndex, yOffset, entry);

			layout.Replace(
				"label/list/entry/" + index,
				() =>
				{
					AddLabelCropped(65, 2 + yOffset, 160, 20, GetLabelHue(index, pIndex, entry), GetLabelText(index, pIndex, entry));
					AddLabelCropped(
						225,
						2 + yOffset,
						150,
						20,
						GetSortLabelHue(index, pIndex, entry),
						GetSortLabelText(index, pIndex, entry));
				});
		}

		protected virtual string GetHelpText()
		{
			return
				String.Format(
					"PvP profiles store all of your battle statistics.\n" +
					"They can be ranked in order of total points, wins or kills.\n" +
					"Do you think you have what it takes to earn the top rank in {0}?",
					ServerList.ServerName).WrapUOHtmlColor(DefaultHtmlColor, false);
		}

		protected override int GetLabelHue(int index, int pageIndex, PvPProfile entry)
		{
			if (index < 3)
			{
				return HighlightHue;
			}

			if (entry != null)
			{
				return Notoriety.GetHue(Notoriety.Compute(User, entry.Owner));
			}

			return base.GetLabelHue(index, pageIndex, null);
		}

		protected override string GetLabelText(int index, int pageIndex, PvPProfile entry)
		{
			if (entry != null && entry.Owner != null)
			{
				if (RankSortOrder != PvPProfileRankOrder.None)
				{
					return String.Format("{0}: {1}", index, entry.Owner.RawName);
				}

				return entry.Owner.RawName;
			}

			return base.GetLabelText(index, pageIndex, entry);
		}

		protected virtual string GetSortLabelText(int index, int pageIndex, PvPProfile entry)
		{
			if (entry != null)
			{
				var key = "Rank";
				long val = index;

				if (RankSortOrder != PvPProfileRankOrder.None)
				{
					key = RankSortOrder.ToString().SpaceWords().ToUpperWords();
					val = AutoPvP.GetSortedValue(RankSortOrder, entry, Season);
				}

				return String.Format("{0}: {1:#,0}", key, val);
			}

			return String.Empty;
		}

		protected virtual int GetSortLabelHue(int index, int pageIndex, PvPProfile entry)
		{
			if (entry != null)
			{
				if (RankSortOrder == PvPProfileRankOrder.None)
				{
					return index <= AutoPvP.CMOptions.Advanced.Seasons.TopListCount ? HighlightHue : TextHue;
				}

				return AutoPvP.GetSortedValue(RankSortOrder, entry, Season) <= 0 ? ErrorHue : TextHue;
			}

			return ErrorHue;
		}

		protected override void SelectEntry(GumpButton button, PvPProfile entry)
		{
			base.SelectEntry(button, entry);

			if (button == null || entry == null || entry.Deleted)
			{
				return;
			}

			Send(new PvPProfileOverviewGump(User, entry, Hide(true), UseConfirmDialog));
		}
	}
}