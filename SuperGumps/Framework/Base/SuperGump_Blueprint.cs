#region Header
//   Vorspire    _,-'/-'/  SuperGump_Blueprint.cs
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
using System.Drawing;
using System.Linq;

using Server.Gumps;
#endregion

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump
	{
		public virtual bool CanMove { get { return Dragable; } set { Dragable = value; } }
		public virtual bool CanClose { get { return Closable; } set { Closable = value; } }
		public virtual bool CanDispose { get { return Disposable; } set { Disposable = value; } }
		public virtual bool CanResize { get { return Resizable; } set { Resizable = value; } }

		public virtual int ModalXOffset { get; set; }
		public virtual int ModalYOffset { get; set; }
		public virtual int XOffset { get; set; }
		public virtual int YOffset { get; set; }

		private Size _InternalSize = new Size(0, 0);

		public Size OuterSize { get { return _InternalSize; } }

		public int OuterWidth { get { return _InternalSize.Width; } }
		public int OuterHeight { get { return _InternalSize.Height; } }

		public void InvalidateSize()
		{
			_InternalSize.Width = Entries.Not(e => e is GumpModal).Max(
				e =>
				{
					int x, w;

					e.TryGetX(out x);
					e.TryGetWidth(out w);

					return x + w;
				}) - (XOffset + ModalXOffset);

			_InternalSize.Height = Entries.Not(e => e is GumpModal).Max(
				e =>
				{
					int y, h;

					e.TryGetY(out y);
					e.TryGetHeight(out h);

					return (y + h);
				}) - (YOffset + ModalYOffset);
		}

		private void InvalidateOffsets()
		{
			Entries.ForEachReverse(
				entry =>
				{
					if (Modal && entry is SuperGumpEntry && ((SuperGumpEntry)entry).IgnoreModalOffset)
					{
						return;
					}

					int x, y;

					if (entry.TryGetPosition(out x, out y))
					{
						entry.TrySetPosition(x + ModalXOffset + XOffset, y + ModalYOffset + YOffset);
					}
				});
		}
	}
}