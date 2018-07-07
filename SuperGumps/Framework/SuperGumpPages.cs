#region Header
//   Vorspire    _,-'/-'/  SuperGumpPages.cs
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

using Server;
using Server.Gumps;
#endregion

namespace VitaNex.SuperGumps
{
	public abstract class SuperGumpPages : SuperGump, ISuperGumpPages
	{
		public static int DefaultEntriesPerPage = 10;

		private int _Page;

		public int PageCount { get; protected set; }

		public abstract int EntryCount { get; }

		public virtual int EntriesPerPage { get; set; }
		public virtual int Page { get { return _Page; } set { _Page = Math.Max(0, Math.Min(PageCount, value)); } }

		public virtual bool HasPrevPage { get { return Page > 0; } }
		public virtual bool HasNextPage { get { return Page < PageCount - 1; } }

		public SuperGumpPages(Mobile user, Gump parent = null, int? x = null, int? y = null)
			: base(user, parent, x, y)
		{
			EntriesPerPage = DefaultEntriesPerPage;
		}

		protected override void Compile()
		{
			InvalidatePageCount();

			base.Compile();
		}

		public virtual void InvalidatePageCount()
		{
			if (EntryCount > EntriesPerPage)
			{
				if (EntriesPerPage > 0)
				{
					PageCount = (int)Math.Ceiling(EntryCount / (double)EntriesPerPage);
					PageCount = Math.Max(1, PageCount);
				}
				else
				{
					PageCount = 1;
				}
			}
			else
			{
				PageCount = 1;
			}

			Page = Math.Max(0, Math.Min(PageCount - 1, Page));
		}

		protected virtual void FirstPage(GumpButton entry)
		{
			FirstPage(true);
		}

		public virtual void FirstPage()
		{
			FirstPage(true);
		}

		public virtual void FirstPage(bool recompile)
		{
			Page = 0;
			Refresh(recompile);
		}

		protected virtual void LastPage(GumpButton entry)
		{
			LastPage(true);
		}

		public virtual void LastPage()
		{
			LastPage(true);
		}

		public virtual void LastPage(bool recompile)
		{
			Page = PageCount - 1;
			Refresh(recompile);
		}

		protected virtual void PreviousPage(GumpButton entry)
		{
			PreviousPage(true);
		}

		public virtual void PreviousPage()
		{
			PreviousPage(true);
		}

		public virtual void PreviousPage(bool recompile)
		{
			--Page;
			Refresh(recompile);
		}

		protected virtual void NextPage(GumpButton entry)
		{
			NextPage(true);
		}

		public virtual void NextPage()
		{
			NextPage(true);
		}

		public virtual void NextPage(bool recompile)
		{
			++Page;
			Refresh(recompile);
		}

		protected override void OnDispose()
		{
			base.OnDispose();

			EntriesPerPage = 0;
			PageCount = 0;
			Page = 0;
		}
	}
}