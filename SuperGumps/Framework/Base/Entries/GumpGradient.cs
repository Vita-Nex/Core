#region Header
//   Vorspire    _,-'/-'/  GumpGradient.cs
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
using System.Drawing;

using Server;
using Server.Gumps;
using Server.Network;
#endregion

namespace VitaNex.SuperGumps
{
	public class GumpGradient : SuperGumpEntry, IGumpEntryVector
	{
		private static readonly Color[] _EmptyColors = new Color[0];
		private static readonly int[] _EmptySizes = new int[0];

		private const string _Format1 = @"{{ htmlgump {0} {1} {2} {3} {4} 0 0 }}";

		private static readonly byte[] _Layout1A = Gump.StringToBuffer("htmlgump");
		private static readonly byte[] _Layout1B = Gump.StringToBuffer(" }{ htmlgump");

		private int _X, _Y;
		private int _Width, _Height;
		private Direction45 _Direction;
		private ColorGradient _Gradient;

		public int X { get { return _X; } set { Delta(ref _X, value); } }
		public int Y { get { return _Y; } set { Delta(ref _Y, value); } }

		public int Width { get { return _Width; } set { Delta(ref _Width, value); } }
		public int Height { get { return _Height; } set { Delta(ref _Height, value); } }

		public Direction45 Direction { get { return _Direction; } set { Delta(ref _Direction, value); } }
		public ColorGradient Gradient { get { return _Gradient; } set { Delta(ref _Gradient, value); } }

		public GumpGradient(int x, int y, int width, int height, Direction45 dirTo, ColorGradient gradient)
		{
			_X = x;
			_Y = y;

			_Width = width;
			_Height = height;

			_Direction = dirTo;
			_Gradient = gradient;
		}

		public virtual bool GetSegments(out Color[] colors, out int[] sizes, out int count)
		{
			switch (_Direction)
			{
				case Direction45.Down:
				case Direction45.Up:
					_Gradient.GetSegments(_Height, out colors, out sizes, out count);
					return true;
				case Direction45.Right:
				case Direction45.Left:
					_Gradient.GetSegments(_Width, out colors, out sizes, out count);
					return true;
				default:
				{
					colors = _EmptyColors;
					sizes = _EmptySizes;
					count = 0;
					return false;
				}
			}
		}

		public override string Compile()
		{
			if (IsEnhancedClient)
			{
				return String.Empty;
			}

			var compiled = String.Empty;

			Color[] colors;
			int[] sizes;
			int count;

			if (GetSegments(out colors, out sizes, out count) && count > 0)
			{
				Color c;
				int s;

				for (int i = 0, o = 0; i < count; i++)
				{
					c = colors[i];
					s = sizes[i];

					if (!c.IsEmpty && c != Color.Transparent && s > 0)
					{
						switch (_Direction)
						{
							case Direction45.Down:
								compiled += Compile(_X, _Y + o, _Width, s, c);
								break;
							case Direction45.Up:
								compiled += Compile(_X, _Y + (_Height - (o + s)), _Width, s, c);
								break;
							case Direction45.Right:
								compiled += Compile(_X + o, _Y, s, _Height, c);
								break;
							case Direction45.Left:
								compiled += Compile(_X + (_Width - (o + s)), _Y, s, _Height, c);
								break;
						}
					}

					o += s;
				}
			}

			if (String.IsNullOrWhiteSpace(compiled))
			{
				compiled = Compile(_X, _Y, _Width, _Height, Color.Transparent);
			}

			return compiled;
		}

		public virtual string Compile(int x, int y, int w, int h, Color c)
		{
			var text = " ";

			if (!c.IsEmpty && c != Color.Transparent)
			{
				text = text.WrapUOHtmlBG(c);
			}

			return String.Format(_Format1, _X, _Y, _Width, _Height, Parent.Intern(text));
		}

		public override void AppendTo(IGumpWriter disp)
		{
			if (IsEnhancedClient)
			{
				AppendEmptyLayout(disp);
				return;
			}

			var first = true;

			Color[] colors;
			int[] sizes;
			int count;

			if (GetSegments(out colors, out sizes, out count))
			{
				Color c;
				int s;

				for (int i = 0, o = 0; i < count; i++)
				{
					c = colors[i];
					s = sizes[i];

					if (!c.IsEmpty && c != Color.Transparent && s > 0)
					{
						switch (_Direction)
						{
							case Direction45.Down:
								AppendTo(disp, ref first, _X, _Y + o, _Width, s, c);
								break;
							case Direction45.Up:
								AppendTo(disp, ref first, _X, _Y + (_Height - (o + s)), _Width, s, c);
								break;
							case Direction45.Right:
								AppendTo(disp, ref first, _X + o, _Y, s, _Height, c);
								break;
							case Direction45.Left:
								AppendTo(disp, ref first, _X + (_Width - (o + s)), _Y, s, _Height, c);
								break;
						}
					}

					o += s;
				}
			}

			if (first)
			{
				AppendTo(disp, ref first, _X, _Y, _Width, _Height, Color.Transparent);
			}
		}

		public virtual void AppendTo(IGumpWriter disp, ref bool first, int x, int y, int w, int h, Color c)
		{
			var text = " ";

			if (!c.IsEmpty && c != Color.Transparent)
			{
				text = text.WrapUOHtmlBG(c);
			}

			disp.AppendLayout(first ? _Layout1A : _Layout1B);
			disp.AppendLayout(x);
			disp.AppendLayout(y);
			disp.AppendLayout(w);
			disp.AppendLayout(h);
			disp.AppendLayout(Parent.Intern(text));
			disp.AppendLayout(false);
			disp.AppendLayout(false);

			first = false;
		}

		public override void Dispose()
		{
			_Gradient = null;

			base.Dispose();
		}
	}
}