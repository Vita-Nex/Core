#region Header
//   Vorspire    _,-'/-'/  Rectangle2DExt.cs
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
#endregion

namespace Server
{
	public static class Rectangle2DExtUtility
	{
		public static Point2D GetRandomPoint(this Rectangle2D bounds)
		{
			return new Point2D(
				Utility.RandomMinMax(bounds.Start.X, bounds.End.X),
				Utility.RandomMinMax(bounds.Start.Y, bounds.End.Y));
		}

		public static Rectangle2D Combine(this IEnumerable<Rectangle2D> bounds)
		{
			int count = 0, minX = Int32.MaxValue, minY = Int32.MaxValue, maxX = Int32.MinValue, maxY = Int32.MinValue;

			foreach (var r in bounds)
			{
				minX = Math.Min(minX, Math.Min(r.Start.X, r.End.X));
				minY = Math.Min(minY, Math.Min(r.Start.Y, r.End.Y));

				maxX = Math.Max(maxX, Math.Max(r.Start.X, r.End.X));
				maxY = Math.Max(maxY, Math.Max(r.Start.Y, r.End.Y));

				++count;
			}

			if (count > 0)
			{
				return new Rectangle2D(new Point2D(minX, minY), new Point2D(maxX, maxY));
			}

			return new Rectangle2D(0, 0, 0, 0);
		}

		public static Rectangle3D ToRectangle3D(this Rectangle2D r, int floor = 0, int roof = 0)
		{
			return new Rectangle3D(r.X, r.Y, floor, r.Width, r.Height, roof);
		}

		public static int GetBoundsHashCode(this Rectangle2D r)
		{
			unchecked
			{
				var hash = r.Width * r.Height;

				hash = (hash * 397) ^ r.Start.GetHashCode();
				hash = (hash * 397) ^ r.End.GetHashCode();

				return hash;
			}
		}

		public static int GetBoundsHashCode(this IEnumerable<Rectangle2D> list)
		{
			unchecked
			{
				return list.Aggregate(0, (hash, r) => (hash * 397) ^ GetBoundsHashCode(r));
			}
		}

		public static int GetArea(this Rectangle2D r)
		{
			return r.Width * r.Height;
		}

		public static Rectangle2D Resize(
			this Rectangle2D r,
			int xOffset = 0,
			int yOffset = 0,
			int wOffset = 0,
			int hOffset = 0)
		{
			var start = r.Start.Clone2D(xOffset, yOffset);
			var end = r.End.Clone2D(xOffset + wOffset, yOffset + hOffset);

			return new Rectangle2D(start, end);
		}

		public static IEnumerable<TEntity> FindEntities<TEntity>(this Rectangle2D r, Map m) where TEntity : IEntity
		{
			if (m == null || m == Map.Internal)
			{
				yield break;
			}

			var o = m.GetObjectsInBounds(r);

			foreach (var e in o.OfType<TEntity>().Where(e => e != null && e.Map == m && r.Contains(e)))
			{
				yield return e;
			}

			o.Free();
		}

		public static IEnumerable<IEntity> FindEntities(this Rectangle2D r, Map m)
		{
			return FindEntities<IEntity>(r, m);
		}

		public static List<TEntity> GetEntities<TEntity>(this Rectangle2D r, Map m) where TEntity : IEntity
		{
			return FindEntities<TEntity>(r, m).ToList();
		}

		public static List<IEntity> GetEntities(this Rectangle2D r, Map m)
		{
			return FindEntities<IEntity>(r, m).ToList();
		}

		public static List<Item> GetItems(this Rectangle2D r, Map m)
		{
			return FindEntities<Item>(r, m).ToList();
		}

		public static List<Mobile> GetMobiles(this Rectangle2D r, Map m)
		{
			return FindEntities<Mobile>(r, m).ToList();
		}

		public static IEnumerable<Point2D> EnumeratePoints(this Rectangle2D r)
		{
			for (var x = r.Start.X; x <= r.End.X; x++)
			{
				for (var y = r.Start.Y; y <= r.End.Y; y++)
				{
					yield return new Point2D(x, y);
				}
			}
		}

		public static void ForEach(this Rectangle2D r, Action<Point2D> action)
		{
			if (action == null)
			{
				return;
			}

			foreach (var p in EnumeratePoints(r))
			{
				action(p);
			}
		}

		public static IEnumerable<Point2D> GetBorder(this Rectangle2D r, int size)
		{
			size = Math.Max(1, size);

			int x, y;
			int x1 = r.Start.X + size, y1 = r.Start.Y + size;
			int x2 = r.End.X - size, y2 = r.End.Y - size;

			for (x = r.Start.X; x <= r.End.X; x++)
			{
				if (x >= x1 || x <= x2)
				{
					continue;
				}

				for (y = r.Start.Y; y <= r.End.Y; y++)
				{
					if (y >= y1 || y <= y2)
					{
						continue;
					}

					yield return new Point2D(x, y);
				}
			}
		}

		public static IEnumerable<Point2D> GetBorder(this Rectangle2D r)
		{
			return GetBorder(r, 1);
		}

		public static bool Intersects(this Rectangle2D r, Rectangle2D or)
		{
			return GetBorder(r).Union(GetBorder(or)).GroupBy(p => p).Any(g => g.Count() > 1);
		}
	}
}