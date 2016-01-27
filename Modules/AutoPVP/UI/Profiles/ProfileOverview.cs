#region Header
//   Vorspire    _,-'/-'/  ProfileOverview.cs
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
using Server.Mobiles;

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPProfileOverviewGump : HtmlPanelGump<PvPProfile>
	{
		public bool UseConfirmDialog { get; set; }

		public PvPProfileOverviewGump(Mobile user, PvPProfile profile, Gump parent = null, bool useConfirm = true)
			: base(user, parent, emptyText: "No profile selected.", title: "PvP Profile Overview", selected: profile)
		{
			UseConfirmDialog = useConfirm;
		}

		protected override void Compile()
		{
			base.Compile();

			if (Selected == null || Selected.Deleted)
			{
				Selected = AutoPvP.EnsureProfile(User as PlayerMobile, true);
			}

			Html = Selected.ToHtmlString(User);
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			if (Selected == null || Selected.Deleted)
			{
				Selected = AutoPvP.EnsureProfile(User as PlayerMobile, true);
			}

			if ((Selected.Owner == User && AutoPvP.CMOptions.Advanced.Profiles.AllowPlayerDelete) ||
				User.AccessLevel >= AutoPvP.Access)
			{
				list.AppendEntry(new ListGumpEntry("Clear Statistics", OnResetStatistics, HighlightHue));
				list.AppendEntry(new ListGumpEntry("Delete Profile", OnDeleteProfile, HighlightHue));
			}

			base.CompileMenuOptions(list);
		}

		protected virtual void OnResetStatistics(GumpButton button)
		{
			if (Selected == null || Selected.Deleted)
			{
				Selected = AutoPvP.EnsureProfile(User as PlayerMobile);
			}

			if (UseConfirmDialog)
			{
				Send(
					new ConfirmDialogGump(
						User,
						Refresh(),
						title: "Reset Profile Statistics?",
						html:
							"All data associated with the profile statistics will be lost.\n" +
							"This action can not be reversed!\nDo you want to continue?",
						onAccept: OnConfirmResetStatistics));
			}
			else
			{
				Selected.History.Entries.Clear();
				Refresh(true);
			}
		}

		protected virtual void OnDeleteProfile(GumpButton button)
		{
			if (Selected == null || Selected.Deleted)
			{
				Selected = AutoPvP.EnsureProfile(User as PlayerMobile);
			}

			if (UseConfirmDialog)
			{
				Send(
					new ConfirmDialogGump(
						User,
						Refresh(),
						title: "Delete Profile?",
						html:
							"All data associated with this profile will be deleted.\nThis action can not be reversed!\nDo you want to continue?",
						onAccept: OnConfirmDeleteProfile));
			}
			else
			{
				Selected.Delete();
				Close();
			}
		}

		protected virtual void OnConfirmResetStatistics(GumpButton button)
		{
			if (Selected != null && !Selected.Deleted)
			{
				Selected.History.Entries.Clear();
			}

			Refresh(true);
		}

		protected virtual void OnConfirmDeleteProfile(GumpButton button)
		{
			if (Selected != null && !Selected.Deleted)
			{
				Selected.Delete();
			}

			Close();
		}
	}
}