#region Header
//   Vorspire    _,-'/-'/  MenuGump.cs
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

using Server;
using Server.Gumps;

using VitaNex.Text;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public class MenuGump : SuperGumpList<ListGumpEntry>
	{
		public int GuessWidth { get; protected set; }

		public GumpButton Clicked { get; set; }

		public ListGumpEntry Selected { get; set; }

		public MenuGump(Mobile user, Gump parent = null, IEnumerable<ListGumpEntry> list = null, GumpButton clicked = null)
			: base(user, parent, DefaultX, DefaultY, list)
		{
			Clicked = clicked;

			if (Clicked != null)
			{
				if (Clicked.Parent != null)
				{
					X = Clicked.Parent.X + Clicked.X;
					Y = Clicked.Parent.Y + Clicked.Y;

					if (Parent == null)
					{
						Parent = Clicked.Parent;
					}
				}
				else if (Parent != null)
				{
					X = Parent.X;
					Y = Parent.Y;
				}
				else
				{
					X = DefaultX;
					Y = DefaultY;
				}
			}
			else if (Parent != null)
			{
				X = Parent.X;
				Y = Parent.Y;
			}
			else
			{
				X = DefaultX;
				Y = DefaultY;
			}

			ForceRecompile = true;
			CanMove = false;
			CanResize = false;
			Modal = true;
		}

		protected virtual void InvalidateWidth()
		{
			double epp = EntriesPerPage;

			GuessWidth = 60;

			if (epp > 0)
			{
				GuessWidth +=
					List.Select((e, i) => GetLabelText(i, (int)Math.Ceiling(i + 1 / epp), e))
						.Select(t => (int)(t.ComputeWidth(UOFont.Font0) * 0.90))
						.DefaultIfEmpty(0)
						.Max();
			}
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			var range = GetListRange();

			layout.Add(
				"background/body/base",
				() =>
				{
					if (SupportsUltimaStore)
					{
						AddBackground(0, 0, GuessWidth, 30 + (range.Count * 30), 40000);
					}
					else
					{
						AddBackground(0, 0, GuessWidth, 30 + (range.Count * 30), 9270);
						AddImageTiled(10, 10, GuessWidth - 20, 10 + (range.Count * 30), 2624);
						//AddAlphaRegion(10, 10, GuessWidth - 20, 10 + (range.Count * 30));
					}
				});

			layout.Add(
				"imagetiled/body/vsep/0",
				() =>
				{
					if (!SupportsUltimaStore)
					{
						AddImageTiled(50, 20, 5, range.Count * 30, 9275);
					}
				});

			CompileEntryLayout(layout, range);

			layout.Add(
				"widget/body/scrollbar",
				() =>
					AddScrollbarH(
						6,
						6,
						PageCount,
						Page,
						PreviousPage,
						NextPage,
						new Rectangle2D(30, 0, GuessWidth - 72, 13),
						new Rectangle2D(0, 0, 28, 13),
						new Rectangle2D(GuessWidth - 40, 0, 28, 13)));
		}

		public virtual void CompileEntryLayout(SuperGumpLayout layout, Dictionary<int, ListGumpEntry> range)
		{
			range.For((i, kv) => CompileEntryLayout(layout, range.Count, kv.Key, i, 25 + (i * 30), kv.Value));
		}

		public virtual void CompileEntryLayout(
			SuperGumpLayout layout,
			int length,
			int index,
			int pIndex,
			int yOffset,
			ListGumpEntry entry)
		{
			layout.Add("button/list/select/" + index, () => AddButton(15, yOffset, 4006, 4007, b => SelectEntry(b, entry)));

			layout.Add(
				"label/list/entry/" + index,
				() =>
					AddLabelCropped(
						65,
						2 + yOffset,
						GuessWidth - 75,
						20,
						GetLabelHue(index, pIndex, entry),
						GetLabelText(index, pIndex, entry)));

			if (pIndex < (length - 1))
			{
				layout.Add(
					"imagetiled/body/hsep/" + index,
					() =>
					{
						if (!SupportsUltimaStore)
						{
							AddImageTiled(10, 25 + yOffset, GuessWidth - 20, 5, 9277);
						}
					});
			}
		}

		protected sealed override void CompileList(List<ListGumpEntry> list)
		{
			var opts = new MenuGumpOptions(list);

			CompileOptions(opts);

			list.Clear();
			list.AddRange(opts);

			InvalidateWidth();
		}

		protected virtual void CompileOptions(MenuGumpOptions list)
		{
			list.Insert(list.Count, new ListGumpEntry("Cancel", Cancel, ErrorHue));
		}

		protected virtual string GetLabelText(int index, int pageIndex, ListGumpEntry entry)
		{
			return entry != null ? entry.Label : "NULL";
		}

		protected virtual int GetLabelHue(int index, int pageIndex, ListGumpEntry entry)
		{
			return entry != null ? entry.Hue : ErrorHue;
		}

		protected virtual void SelectEntry(GumpButton button, ListGumpEntry entry)
		{
			Selected = entry;

			if (entry == null || entry.Handler == null)
			{
				Close();
				return;
			}

			entry.Handler(button);
		}

		protected virtual void Cancel(GumpButton button)
		{
			Close();
		}
	}
}