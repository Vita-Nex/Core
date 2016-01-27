#region Header
//   Vorspire    _,-'/-'/  FilterGump.cs
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
using System.Drawing;

using Server.Gumps;
using Server.Mobiles;

using VitaNex;
using VitaNex.SuperGumps;
#endregion

namespace Server
{
	public class FilterGump : SuperGumpList<FilterGumpEntry>
	{
		private PlayerMobile _Owner;

		public PlayerMobile Owner { get { return _Owner == User ? _Owner : (_Owner = User as PlayerMobile); } }

		public bool UseOwnFilter
		{
			get { return Owner != null && Owner.UseOwnFilter; }
			set
			{
				if (Owner != null)
				{
					Owner.UseOwnFilter = value;
				}
			}
		}

		public IFilter OwnerFilter { get { return Owner != null ? Owner.GetFilter(MainFilter.GetType()) : null; } }

		public IFilter Filter { get { return UseOwnFilter ? OwnerFilter ?? MainFilter : MainFilter; } }

		public IFilter MainFilter { get; private set; }

		public Action<Mobile, IFilter> ApplyHandler { get; set; }

		public FilterGump(Mobile user, Gump parent, IFilter filter, Action<Mobile, IFilter> onApply)
			: base(user, parent)
		{
			MainFilter = filter;
			ApplyHandler = onApply;

			CanMove = true;
			CanResize = false;
			CanDispose = true;
			CanClose = true;

			ForceRecompile = true;

			EntriesPerPage = 32;
		}

		protected override bool OnBeforeSend()
		{
			if (MainFilter == null)
			{
				return false;
			}

			return base.OnBeforeSend();
		}

		protected override void CompileList(List<FilterGumpEntry> list)
		{
			list.Clear();

			var col = 0;
			var row = 0;
			var max = 12;

			string c = null;

			foreach (var e in Filter.Options)
			{
				if (row > 0 && row % max == 0)
				{
					if (++col > 2)
					{
						col = 0;
					}

					row = 0;
					max = col == 2 ? 9 : 12;
				}

				if (c == null || !Insensitive.Equals(c, e.Category) || row == 0)
				{
					if (row + 1 >= max)
					{
						if (++col > 2)
						{
							col = 0;
						}

						row = 0;
						max = col == 2 ? 9 : 12;
					}

					list.Add(new FilterGumpEntry(e, true, col, row++));
				}

				list.Add(new FilterGumpEntry(e, false, col, row++));

				c = e.Category;
			}

			base.CompileList(list);
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Add(
				"bg",
				() =>
				{
					AddBackground(0, 390, 600, 50, 9270);
					AddImageTiled(10, 400, 580, 30, 2624);
					AddAlphaRegion(10, 400, 580, 30);

					AddBackground(0, 0, 600, 400, 9270);
					AddImageTiled(10, 10, 580, 380, 2624);
					//AddAlphaRegion(10, 10, 580, 380);

					AddImageTiled(199, 10, 3, 360, 9275);
					AddImageTiled(399, 10, 3, 360, 9275);
					AddImageTiled(400, 275, 190, 3, 9277);
					AddImageTiled(10, 367, 580, 3, 9277);
				});

			layout.Add(
				"title",
				() =>
				{
					var title = String.Format("Filter Options: {0} ({1})", Filter.Name, UseOwnFilter ? "Personal" : "Public");
					title = title.WrapUOHtmlCenter().WrapUOHtmlColor(Color.PaleGoldenrod);

					AddHtml(5, 405, 590, 30, title, false, false);
				});

			layout.Add(
				"scroll",
				() =>
				{
					AddScrollbar(
						Axis.Horizontal,
						10,
						370,
						PageCount,
						Page,
						PreviousPage,
						NextPage,
						30,
						0,
						520,
						22,
						9264,
						9354,
						0,
						0,
						30,
						22,
						4015,
						4016,
						4014,
						550,
						0,
						30,
						22,
						4006,
						4007,
						4005);

					if (PageCount > 0)
					{
						AddImageNumber(300, 367, Page + 1, 0, Axis.Horizontal);
					}
				});

			var range = GetListRange();

			foreach (var e in range.Values)
			{
				if (e.IsCategory)
				{
					CompileCategoryLayout(layout, e);
				}
				else
				{
					CompileEntryLayout(layout, e);
				}
			}

			layout.Add(
				"cpanel",
				() =>
				{
					AddButton(
						408,
						280,
						4006,
						4007,
						b =>
						{
							UseOwnFilter = !UseOwnFilter;
							Refresh(true);
						}); // Use [My|Book] Filter
					AddHtml(443, 282, 140, 40, FormatText(Color.White, "Use {0} Filter", UseOwnFilter ? "Main" : "My"), false, false);

					AddButton(
						408,
						310,
						4021,
						4022,
						b =>
						{
							Filter.Clear();
							Refresh(true);
						}); //Clear
					AddHtml(443, 312, 140, 40, FormatText(Color.White, "Clear"), false, false);

					AddButton(408, 340, 4024, 4025, Close); //Apply
					AddHtml(443, 342, 140, 40, FormatText(Color.White, "Apply"), false, false);
				});
		}

		protected override void OnClosed(bool all)
		{
			base.OnClosed(all);

			if (ApplyHandler != null)
			{
				ApplyHandler(User, Filter);
			}
		}

		protected virtual void CompileCategoryLayout(SuperGumpLayout layout, FilterGumpEntry e)
		{
			layout.Add(
				"entries/" + e.Col + "," + e.Row + "/category",
				() =>
				{
					var x = 15 + (e.Col == 1 ? 195 : e.Col == 2 ? 395 : 0);
					var y = 12 + (e.Row * 30);

					if (e.Row > 0)
					{
						AddImageTiled(x - 10, y - 5, e.Col == 1 ? 200 : 190, 3, 9277);
					}

					AddHtml(x, y + 2, 175, 40, FormatTitle(Color.Gold, e.Option.Category), false, false);
				});
		}

		protected virtual void CompileEntryLayout(SuperGumpLayout layout, FilterGumpEntry e)
		{
			layout.Add(
				"entries/" + e.Col + "," + e.Row + "/option",
				() =>
				{
					var x = 15 + (e.Col == 1 ? 195 : e.Col == 2 ? 395 : 0);
					var y = 12 + (e.Row * 30);

					var s = e.Option.IsSelected(Filter);

					if (s)
					{
						AddImage(x, y, 4012);
						AddHtml(x + 35, y + 2, 175, 40, FormatText(Color.LawnGreen, e.Option.Name), false, false);
					}
					else
					{
						AddButton(x, y, 4011, 4013, b => Refresh(e.Option.Select(Filter)));
						AddHtml(x + 35, y + 2, 175, 40, FormatText(Color.White, e.Option.Name), false, false);
					}
				});
		}

		protected string FormatTitle(Color c, string text, params object[] args)
		{
			return String.Format(text, args).ToUpper().WrapUOHtmlBold().WrapUOHtmlColor(c, false);
		}

		protected string FormatText(Color c, string text, params object[] args)
		{
			return String.Format(text, args).ToUpper().WrapUOHtmlColor(c, false);
		}
	}
}