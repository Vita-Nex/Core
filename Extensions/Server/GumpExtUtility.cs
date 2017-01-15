#region Header
//   Vorspire    _,-'/-'/  GumpExtUtility.cs
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
using System.Reflection;

using VitaNex.SuperGumps;
#endregion

namespace Server.Gumps
{
	public static class GumpExtUtility
	{
		private sealed class Description
		{
			public PropertyInfo X { get; private set; }
			public PropertyInfo Y { get; private set; }
			public PropertyInfo Width { get; private set; }
			public PropertyInfo Height { get; private set; }

			public Description(PropertyInfo x, PropertyInfo y, PropertyInfo w, PropertyInfo h)
			{
				X = x;
				Y = y;
				Width = w;
				Height = h;
			}

			public override string ToString()
			{
				return String.Format(
					"({0}, {1})+({2}, {3})",
					X != null ? X.ToString() : "Null X",
					Y != null ? Y.ToString() : "Null Y",
					Width != null ? Width.ToString() : "Null Width",
					Height != null ? Height.ToString() : "Null Height");
			}
		}

		private static readonly Dictionary<Type, Description> _PositionProps;

		static GumpExtUtility()
		{
			_PositionProps = new Dictionary<Type, Description>();

			PropertyInfo x, y, w, h;

			foreach (var t in typeof(GumpEntry).FindChildren(t => !t.IsAbstract))
			{
				if (!t.HasInterface<IGumpEntryPoint>())
				{
					x = t.GetProperty("X", typeof(int));
					y = t.GetProperty("Y", typeof(int));
				}
				else
				{
					x = y = null;
				}

				if (!t.HasInterface<IGumpEntrySize>())
				{
					w = t.GetProperty("Width", typeof(int));
					h = t.GetProperty("Height", typeof(int));
				}
				else
				{
					w = h = null;
				}

				if (x != null || y != null || w != null || h != null)
				{
					_PositionProps[t] = new Description(x, y, w, h);
				}
			}
		}

		public static bool TryGetX(this GumpEntry e, out int x)
		{
			if (e is IGumpEntryPoint)
			{
				x = ((IGumpEntryPoint)e).X;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) && props.X != null)
			{
				x = (int)props.X.GetValue(e, null);
				return true;
			}

			x = 0;
			return false;
		}

		public static bool TrySetX(this GumpEntry e, int x)
		{
			if (e is IGumpEntryPoint)
			{
				((IGumpEntryPoint)e).X = x;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) && props.X != null)
			{
				props.X.SetValue(e, x, null);
				return true;
			}

			return false;
		}

		public static bool TryOffsetX(this GumpEntry e, int x)
		{
			int ox;

			if (TryGetX(e, out ox))
			{
				return TrySetX(e, ox + x);
			}

			return false;
		}

		public static bool TryGetY(this GumpEntry e, out int y)
		{
			if (e is IGumpEntryPoint)
			{
				y = ((IGumpEntryPoint)e).Y;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) && props.Y != null)
			{
				y = (int)props.Y.GetValue(e, null);
				return true;
			}

			y = 0;
			return false;
		}

		public static bool TrySetY(this GumpEntry e, int y)
		{
			if (e is IGumpEntryPoint)
			{
				((IGumpEntryPoint)e).Y = y;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) && props.Y != null)
			{
				props.Y.SetValue(e, y, null);
				return true;
			}

			return false;
		}

		public static bool TryOffsetY(this GumpEntry e, int y)
		{
			int oy;

			if (TryGetY(e, out oy))
			{
				return TrySetY(e, oy + y);
			}

			return false;
		}

		public static bool TryGetWidth(this GumpEntry e, out int width)
		{
			if (e is IGumpEntrySize)
			{
				width = ((IGumpEntrySize)e).Width;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) && props.Width != null)
			{
				width = (int)props.Width.GetValue(e, null);
				return true;
			}

			width = 0;
			return false;
		}

		public static bool TrySetWidth(this GumpEntry e, int width)
		{
			if (e is IGumpEntrySize)
			{
				((IGumpEntrySize)e).Width = width;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) && props.Width != null)
			{
				props.Width.SetValue(e, width, null);
				return true;
			}

			return false;
		}

		public static bool TryOffsetWidth(this GumpEntry e, int width)
		{
			int ow;

			if (TryGetWidth(e, out ow))
			{
				return TrySetWidth(e, ow + width);
			}

			return false;
		}

		public static bool TryGetHeight(this GumpEntry e, out int height)
		{
			if (e is IGumpEntrySize)
			{
				height = ((IGumpEntrySize)e).Height;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) && props.Height != null)
			{
				height = (int)props.Height.GetValue(e, null);
				return true;
			}

			height = 0;
			return false;
		}

		public static bool TrySetHeight(this GumpEntry e, int height)
		{
			if (e is IGumpEntrySize)
			{
				((IGumpEntrySize)e).Height = height;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) && props.Height != null)
			{
				props.Height.SetValue(e, height, null);
				return true;
			}

			return false;
		}

		public static bool TryOffsetHeight(this GumpEntry e, int height)
		{
			int oh;

			if (TryGetHeight(e, out oh))
			{
				return TrySetHeight(e, oh + height);
			}

			return false;
		}

		public static bool TryGetPosition(this GumpEntry e, out int x, out int y)
		{
			if (e is IGumpEntryPoint)
			{
				x = ((IGumpEntryPoint)e).X;
				y = ((IGumpEntryPoint)e).Y;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) && props.X != null && props.Y != null)
			{
				x = (int)props.X.GetValue(e, null);
				y = (int)props.Y.GetValue(e, null);
				return true;
			}

			x = y = 0;
			return false;
		}

		public static bool TrySetPosition(this GumpEntry e, int x, int y)
		{
			if (e is IGumpEntryPoint)
			{
				((IGumpEntryPoint)e).X = x;
				((IGumpEntryPoint)e).Y = y;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) && props.X != null && props.Y != null)
			{
				props.X.SetValue(e, x, null);
				props.Y.SetValue(e, y, null);
				return true;
			}

			return false;
		}

		public static bool TryOffsetPosition(this GumpEntry e, int x, int y)
		{
			int ox, oy;

			if (TryGetPosition(e, out ox, out oy))
			{
				return TrySetPosition(e, ox + x, oy + y);
			}

			return false;
		}

		public static bool TryGetSize(this GumpEntry e, out int width, out int height)
		{
			if (e is IGumpEntrySize)
			{
				width = ((IGumpEntrySize)e).Width;
				height = ((IGumpEntrySize)e).Height;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) && props.Width != null && props.Height != null)
			{
				width = (int)props.Width.GetValue(e, null);
				height = (int)props.Height.GetValue(e, null);
				return true;
			}

			width = height = 0;
			return false;
		}

		public static bool TrySetSize(this GumpEntry e, int width, int height)
		{
			if (e is IGumpEntrySize)
			{
				((IGumpEntrySize)e).Width = width;
				((IGumpEntrySize)e).Height = height;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) && props.Width != null && props.Height != null)
			{
				props.Width.SetValue(e, width, null);
				props.Height.SetValue(e, height, null);
				return true;
			}

			return false;
		}

		public static bool TryOffsetSize(this GumpEntry e, int width, int height)
		{
			int ow, oh;

			if (TryGetSize(e, out ow, out oh))
			{
				return TrySetSize(e, ow + width, oh + height);
			}

			return false;
		}

		public static bool TryGetBounds(this GumpEntry e, out int x, out int y, out int width, out int height)
		{
			if (e is IGumpEntryVector)
			{
				x = ((IGumpEntryVector)e).X;
				y = ((IGumpEntryVector)e).Y;
				width = ((IGumpEntryVector)e).Width;
				height = ((IGumpEntryVector)e).Height;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) //
				&& props.X != null && props.Y != null && props.Width != null && props.Height != null)
			{
				x = (int)props.X.GetValue(e, null);
				y = (int)props.Y.GetValue(e, null);
				width = (int)props.Width.GetValue(e, null);
				height = (int)props.Height.GetValue(e, null);
				return true;
			}

			x = y = width = height = 0;
			return false;
		}

		public static bool TrySetBounds(this GumpEntry e, int x, int y, int width, int height)
		{
			if (e is IGumpEntryVector)
			{
				((IGumpEntryVector)e).X = x;
				((IGumpEntryVector)e).Y = y;
				((IGumpEntryVector)e).Width = width;
				((IGumpEntryVector)e).Height = height;
				return true;
			}

			Description props;

			if (_PositionProps.TryGetValue(e.GetType(), out props) //
				&& props.X != null && props.Y != null && props.Width != null && props.Height != null)
			{
				props.X.SetValue(e, x, null);
				props.Y.SetValue(e, y, null);
				props.Width.SetValue(e, width, null);
				props.Height.SetValue(e, height, null);
				return true;
			}

			return false;
		}

		public static bool TryOffsetBounds(this GumpEntry e, int x, int y, int width, int height)
		{
			int ox, oy, ow, oh;

			if (TryGetBounds(e, out ox, out oy, out ow, out oh))
			{
				return TrySetBounds(e, ox + x, oy + y, ow + width, oh + height);
			}

			return false;
		}

		public static Rectangle2D GetBounds(this Gump g)
		{
			int x = g.X, y = g.Y, w = 0, h = 0;

			if (g is SuperGump)
			{
				var sg = (SuperGump)g;

				x += sg.XOffset;
				y += sg.YOffset;

				if (sg.Modal)
				{
					x += sg.ModalXOffset;
					y += sg.ModalYOffset;
				}

				w = sg.OuterWidth;
				h = sg.OuterHeight;
			}
			else
			{
				foreach (var e in g.Entries)
				{
					int ex, ey;
					e.TryGetPosition(out ex, out ey);

					int ew, eh;
					e.TryGetSize(out ew, out eh);

					w = Math.Max(ex + ew, w);
					h = Math.Max(ey + eh, h);
				}
			}

			return new Rectangle2D(x, y, w, h);
		}
	}
}