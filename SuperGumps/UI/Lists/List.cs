#region Header
//   Vorspire    _,-'/-'/  List.cs
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
using System.Linq;
using System.Text.RegularExpressions;

using Server;
using Server.Gumps;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public class ListGump<T> : SuperGumpList<T>
	{
		public const string DefaultTitle = "List View";
		public const string DefaultEmptyText = "No entries to display.";

		protected bool WasModal { get; set; }

		public virtual string Title { get; set; }

		public virtual string EmptyText { get; set; }

		public virtual bool Minimized { get; set; }

		public virtual T Selected { get; set; }

		public virtual bool CanSearch { get; set; }
		public virtual string SearchText { get; set; }
		public virtual List<T> SearchResults { get; set; }

		public virtual MenuGumpOptions Options { get; set; }

		public MenuGump Menu { get; private set; }

		public override int EntryCount { get { return IsSearching() ? SearchResults.Count : base.EntryCount; } }

		public ListGump(
			Mobile user,
			Gump parent = null,
			int? x = null,
			int? y = null,
			IEnumerable<T> list = null,
			string emptyText = null,
			string title = null,
			IEnumerable<ListGumpEntry> opts = null)
			: base(user, parent, x, y, list)
		{
			EmptyText = emptyText ?? DefaultEmptyText;
			Title = title ?? DefaultTitle;
			
			Minimized = false;
			
			CanMove = false;
			CanSearch = true;

			Options = new MenuGumpOptions(opts);
		}

		public override void AssignCollections()
		{
			if (SearchResults == null)
			{
				SearchResults = new List<T>(0x20);
			}

			base.AssignCollections();
		}

		public virtual bool IsSearching()
		{
			return CanSearch && !String.IsNullOrWhiteSpace(SearchText);
		}

		protected override void Compile()
		{
			base.Compile();

			if (Options == null)
			{
				Options = new MenuGumpOptions();
			}
			else
			{
				Options.Clear();
			}

			CompileMenuOptions(Options);
		}

		protected override void CompileList(List<T> list)
		{
			SearchResults.Clear();

			if (IsSearching())
			{
				SearchResults.AddRange(
					list.Where(o => o != null && Regex.IsMatch(GetSearchKeyFor(o), Regex.Escape(SearchText), RegexOptions.IgnoreCase)));

				if (Sorted)
				{
					SearchResults.Sort(SortCompare);
				}
			}

			base.CompileList(list);
		}

		public virtual string GetSearchKeyFor(T key)
		{
			return key != null ? key.ToString() : "NULL";
		}

		protected virtual void OnClearSearch(GumpButton button)
		{
			SearchText = null;
			Refresh(true);
		}

		protected virtual void OnNewSearch(GumpButton button)
		{
			Send(
				new InputDialogGump(User, this)
				{
					Title = "Search",
					Html = "Search " + Title + ".\nRegular Expressions are supported.",
					Limit = 100,
					Callback = (subBtn, input) =>
					{
						SearchText = input;
						Page = 0;
						Refresh(true);
					}
				});
		}

		protected virtual void CompileMenuOptions(MenuGumpOptions list)
		{
			if (Minimized)
			{
				list.Replace("Minimize", new ListGumpEntry("Maximize", Maximize));
			}
			else
			{
				list.Replace("Maximize", new ListGumpEntry("Minimize", Minimize));
			}

			if (CanSearch)
			{
				if (IsSearching())
				{
					list.Replace("New Search", new ListGumpEntry("Clear Search", OnClearSearch));
				}
				else
				{
					list.Replace("Clear Search", new ListGumpEntry("New Search", OnNewSearch));
				}
			}

			list.AppendEntry(new ListGumpEntry("Refresh", Refresh));

			if (CanClose)
			{
				list.AppendEntry(new ListGumpEntry("Exit", Close));
			}
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Add(
				"background/header/base",
				() =>
				{
					AddBackground(0, 0, 420, 50, 9270);
					AddImageTiled(10, 10, 400, 30, 2624);
					//AddAlphaRegion(10, 10, 400, 30);
				});

			layout.Add(
				"button/header/options",
				() =>
				{
					AddButton(15, 15, 2008, 2007, ShowOptionMenu);
					AddTooltip(1015326);
				});

			layout.Add(
				"button/header/minimize",
				() =>
				{
					if (Minimized)
					{
						AddButton(390, 20, 10740, 10742, Maximize);
						AddTooltip(3002086);
					}
					else
					{
						AddButton(390, 20, 10741, 10742, Minimize);
						AddTooltip(3002085);
					}
				});

			layout.Add(
				"label/header/title",
				() => AddLabelCropped(90, 15, 285, 20, GetTitleHue(), String.IsNullOrWhiteSpace(Title) ? DefaultTitle : Title));

			if (Minimized)
			{
				return;
			}

			layout.Add("imagetiled/body/spacer", () => AddImageTiled(0, 50, 420, 10, 9274));

			var range = GetListRange();

			if (range.Count == 0)
			{
				layout.Add(
					"background/body/base",
					() =>
					{
						AddBackground(0, 55, 420, 50, 9270);
						AddImageTiled(10, 65, 400, 30, 2624);
						//AddAlphaRegion(10, 65, 400, 30); 
					});

				layout.Add(
					"label/list/empty",
					() => AddLabelCropped(15, 72, 325, 20, ErrorHue, String.IsNullOrEmpty(EmptyText) ? DefaultEmptyText : EmptyText));
			}
			else
			{
				layout.Add(
					"background/body/base",
					() =>
					{
						AddBackground(0, 55, 420, 20 + (range.Count * 30), 9270);
						AddImageTiled(10, 65, 400, (range.Count * 30), 2624);
						//AddAlphaRegion(10, 65, 400, (range.Count * 30));
					});

				layout.Add("imagetiled/body/vsep/0", () => AddImageTiled(50, 65, 5, (range.Count * 30), 9275));

				CompileEntryLayout(layout, range);
			}

			layout.Add(
				"widget/body/scrollbar",
				() =>
					AddScrollbarH(
						6,
						46,
						PageCount,
						Page,
						PreviousPage,
						NextPage,
						new Rectangle2D(30, 0, 348, 13),
						new Rectangle2D(0, 0, 28, 13),
						new Rectangle2D(380, 0, 28, 13)));
		}

		public override Dictionary<int, T> GetListRange(int index, int length)
		{
			if (!IsSearching())
			{
				return base.GetListRange(index, length);
			}

			index = Math.Max(0, Math.Min(EntryCount, index));
			length = Math.Max(0, Math.Min(EntryCount - index, length));

			var d = new Dictionary<int, T>(length);

			while (--length >= 0 && SearchResults.InBounds(index))
			{
				d.Add(index, SearchResults[index]);

				++index;
			}

			return d;
		}

		protected virtual void CompileEntryLayout(SuperGumpLayout layout, Dictionary<int, T> range)
		{
			var i = 0;

			foreach (var kv in range)
			{
				CompileEntryLayout(layout, range.Count, kv.Key, i, 70 + (i * 30), kv.Value);

				++i;
			}
		}

		protected virtual void CompileEntryLayout(
			SuperGumpLayout layout,
			int length,
			int index,
			int pIndex,
			int yOffset,
			T entry)
		{
			layout.Add("button/list/select/" + index, () => AddButton(15, yOffset, 4006, 4007, btn => SelectEntry(btn, entry)));

			layout.Add(
				"label/list/entry/" + index,
				() =>
					AddLabelCropped(65, 2 + yOffset, 325, 20, GetLabelHue(index, pIndex, entry), GetLabelText(index, pIndex, entry)));

			if (pIndex < (length - 1))
			{
				layout.Add("imagetiled/body/hsep/" + index, () => AddImageTiled(12, 25 + yOffset, 400, 5, 9277));
			}
		}

		protected virtual void Minimize(GumpButton entry = null)
		{
			Minimized = true;

			if (Modal)
			{
				WasModal = true;
			}

			Modal = false;

			Refresh(true);
		}

		protected virtual void Maximize(GumpButton entry = null)
		{
			Minimized = false;

			if (WasModal)
			{
				Modal = true;
			}

			WasModal = false;

			Refresh(true);
		}

		protected virtual int GetTitleHue()
		{
			return HighlightHue;
		}

		protected virtual string GetLabelText(int index, int pageIndex, T entry)
		{
			return entry != null ? entry.ToString() : "NULL";
		}

		protected virtual int GetLabelHue(int index, int pageIndex, T entry)
		{
			return entry != null ? TextHue : ErrorHue;
		}

		protected virtual void SelectEntry(GumpButton button, T entry)
		{
			Selected = entry;
		}

		protected virtual void ShowOptionMenu(GumpButton button)
		{
			if (User != null && !User.Deleted && Options != null && Options.Count > 0)
			{
				Send(new MenuGump(User, Refresh(), Options, button));
			}
		}
	}

	public class ListGump<T, U> : ListGump<KeyValuePair<T, U>>
	{
		public ListGump(
			Mobile user,
			Gump parent = null,
			int? x = null,
			int? y = null,
			IEnumerable<KeyValuePair<T, U>> list = null,
			string emptyText = null,
			string title = null,
			IEnumerable<ListGumpEntry> opts = null)
			: base(user, parent, x, y, list, emptyText, title, opts)
		{ }
	}
}