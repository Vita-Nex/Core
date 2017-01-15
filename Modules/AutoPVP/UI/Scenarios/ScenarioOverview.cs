#region Header
//   Vorspire    _,-'/-'/  ScenarioOverview.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server;
using Server.Gumps;

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPScenarioOverviewGump : HtmlPanelGump<PvPScenario>
	{
		public PvPScenarioOverviewGump(Mobile user, PvPScenario scenario, Gump parent = null, bool useConfirm = true)
			: base(user, parent, emptyText: "No scenario selected.", title: "PvP Scenario Overview", selected: scenario)
		{
			UseConfirmDialog = useConfirm;
		}

		public bool UseConfirmDialog { get; set; }

		protected override void Compile()
		{
			base.Compile();

			if (Selected == null)
			{
				return;
			}

			Html = Selected.ToHtmlString(User);
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			if (Selected != null)
			{
				if (User.AccessLevel >= AutoPvP.Access)
				{
					list.AppendEntry(
						new ListGumpEntry(
							"Create Battle",
							b =>
							{
								if (UseConfirmDialog)
								{
									new ConfirmDialogGump(User, this)
									{
										Title = "Create New Battle?",
										Html = "This action will create a new battle from the selected scenario.\nDo you want to continue?",
										AcceptHandler = OnConfirmCreateBattle,
										CancelHandler = Refresh
									}.Send();
								}
								else
								{
									OnConfirmCreateBattle(b);
								}
							},
							HighlightHue));
				}
			}

			base.CompileMenuOptions(list);
		}

		protected virtual void OnConfirmCreateBattle(GumpButton button)
		{
			if (Selected == null)
			{
				Close();
				return;
			}

			var battle = AutoPvP.CreateBattle(Selected);

			if (UseConfirmDialog)
			{
				new ConfirmDialogGump(User, Refresh(true))
				{
					Title = "View New Battle?",
					Html = "Your new battle has been created.\nDo you want to view it now?",
					AcceptHandler = b => new PvPBattleOverviewGump(User, Hide(true), battle).Send(),
					CancelHandler = Refresh
				}.Send();
			}
			else
			{
				new PvPBattleOverviewGump(User, Hide(true), battle).Send();
			}
		}
	}
}