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
			int x1 = 0, y1 = 0, x2 = 0, y2 = 0;

			foreach (var e in Entries.Not(e => e is GumpModal))
			{
				int ex, ey, ew, eh;

				if (!e.TryGetBounds(out ex, out ey, out ew, out eh))
				{
					continue;
				}

				x1 = Math.Min(x1, ex);
				y1 = Math.Min(y1, ey);

				x2 = Math.Max(x2, ex + ew);
				y2 = Math.Max(y2, ey + eh);
			}

			_InternalSize.Width = Math.Max(0, x2 - x1);
			_InternalSize.Height = Math.Max(0, y2 - y1);
		}

		private void InvalidateOffsets()
		{
			Entries.ForEachReverse(
				e =>
				{
					var x = XOffset;
					var y = YOffset;

					if (Modal && (!(e is SuperGumpEntry) || !((SuperGumpEntry)e).IgnoreModalOffset))
					{
						x += ModalXOffset;
						y += ModalYOffset;
					}

					if (x != 0 || y != 0)
					{
						e.TryOffsetPosition(x, y);
					}
				});
		}
	}
}