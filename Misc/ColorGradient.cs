#region Header
//   Vorspire    _,-'/-'/  ColorGradient.cs
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
using System.Linq;
#endregion

namespace VitaNex
{
	public class ColorGradient : List<Color>
	{
		public static ColorGradient CreateInstance(params Color[] colors)
		{
			return CreateInstance(0, colors);
		}

		public static ColorGradient CreateInstance(int fade, params Color[] colors)
		{
			if (fade <= 0 || colors.Length < 2)
			{
				return new ColorGradient(colors);
			}

			var gradient = new ColorGradient(colors.Length + ((fade - 1) * colors.Length));

			Color c1, c2;
			double f;

			for (var i = 0; colors.InBounds(i); i++)
			{
				c1 = colors[i];

				if (!colors.InBounds(i + 1))
				{
					gradient.Add(c1);
					break;
				}

				c2 = colors[i + 1];

				for (f = 0; f < fade; f++)
				{
					gradient.Add(c1.Interpolate(c2, f / fade));
				}
			}

			gradient.Free(false);

			return gradient;
		}

		public ColorGradient(int capacity)
			: base(capacity)
		{ }

		public ColorGradient(IEnumerable<Color> colors)
			: base(colors.Ensure())
		{ }

		public void GetSegments(int size, out Color[] colors, out int[] sizes, out int count)
		{
			count = Count;
			colors = new Color[count];
			sizes = new int[count];

			if (count == 0)
			{
				return;
			}

			var chunk = (int)Math.Ceiling(size / (double)count);

			for (var i = 0; i < count; i++)
			{
				colors[i] = this[i];
				sizes[i] = chunk;
			}

			var total = sizes.Sum();

			if (total > size)
			{
				var diff = total - size;

				while (diff > 0)
				{
					var share = (int)Math.Ceiling(diff / (double)count);

					for (var i = count - 1; i >= 0; i--)
					{
						sizes[i] -= share;

						diff -= share;

						if (diff <= 0)
						{
							break;
						}
					}
				}
			}
			else if (total < size)
			{
				var diff = size - total;

				while (diff > 0)
				{
					var share = (int)Math.Ceiling(diff / (double)count);

					for (var i = 0; i < count; i++)
					{
						sizes[i] += share;

						diff -= share;

						if (diff <= 0)
						{
							break;
						}
					}
				}
			}
		}
	}
}