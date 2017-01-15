#region Header
//   Vorspire    _,-'/-'/  Rectangle3DExt.cs
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
	public static class Rectangle3DExtUtility
	{
		public static Point3D GetRandomPoint(this Rectangle3D bounds)
		{
			return new Point3D(
				Utility.RandomMinMax(bounds.Start.X, bounds.End.X),
				Utility.RandomMinMax(bounds.Start.Y, bounds.End.Y),
				Utility.RandomMinMax(bounds.Start.Z, bounds.End.Z));
		}
		public static Point2D GetRandomPoint2D(this Rectangle3D bounds)
		{
			return new Point2D(
				Utility.RandomMinMax(bounds.Start.X, bounds.End.X),
				Utility.RandomMinMax(bounds.Start.Y, bounds.End.Y));
		}

		public static Rectangle2D Combine2D(this IEnumerable<Rectangle3D> bounds)
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

		public static Rectangle3D Combine(this IEnumerable<Rectangle3D> bounds)
		{
			int count = 0,
				minX = Int32.MaxValue,
				minY = Int32.MaxValue,
				maxX = Int32.MinValue,
				maxY = Int32.MinValue,
				minZ = Region.MaxZ,
				maxZ = Region.MinZ;

			foreach (var r in bounds)
			{
				minX = Math.Min(minX, Math.Min(r.Start.X, r.End.X));
				minY = Math.Min(minY, Math.Min(r.Start.Y, r.End.Y));
				minZ = Math.Min(minZ, Math.Min(r.Start.Z, r.End.Z));

				maxX = Math.Max(maxX, Math.Max(r.Start.X, r.End.X));
				maxY = Math.Max(maxY, Math.Max(r.Start.Y, r.End.Y));
				maxZ = Math.Max(maxZ, Math.Max(r.Start.Z, r.End.Z));

				++count;
			}

			if (count > 0)
			{
				return new Rectangle3D(new Point3D(minX, minY, minZ), new Point3D(maxX, maxY, maxZ));
			}

			return new Rectangle3D(0, 0, 0, 0, 0, 0);
		}

		public static IEnumerable<Rectangle3D> ZFix(this IEnumerable<Rectangle3D> rects)
		{
			if (rects == null)
			{
				yield break;
			}

			foreach (var r in rects.Select(r => r.ZFix()))
			{
				yield return r;
			}
		}

		public static Rectangle3D ZFix(this Rectangle3D rect)
		{
			Point3D start = rect.Start, end = rect.End;

			start.Z = Region.MinZ;
			end.Z = Region.MaxZ;
			rect.Start = start;
			rect.End = end;

			return new Rectangle3D(start, end);
		}

		public static Rectangle2D ToRectangle2D(this Rectangle3D r)
		{
			return new Rectangle2D(r.Start, r.End);
		}

		public static int GetBoundsHashCode(this Rectangle3D r)
		{
			unchecked
			{
				var hash = r.Width * r.Height * r.Depth;

				hash = (hash * 397) ^ r.Start.GetHashCode();
				hash = (hash * 397) ^ r.End.GetHashCode();

				return hash;
			}
		}

		public static int GetBoundsHashCode(this IEnumerable<Rectangle3D> list)
		{
			unchecked
			{
				return list.Aggregate(0, (hash, r) => (hash * 397) ^ GetBoundsHashCode(r));
			}
		}

		public static bool Contains(this RegionRect[] rects, IPoint3D p)
		{
			return rects.Any(rect => Contains(rect.Rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this RegionRect[] rects, Point3D p)
		{
			return rects.Any(rect => Contains(rect.Rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this Rectangle3D[] rects, IPoint3D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this Rectangle3D[] rects, Point3D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this Rectangle3D[] rects, IPoint2D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y));
		}

		public static bool Contains(this Rectangle3D[] rects, Point2D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y));
		}

		public static bool Contains(this List<RegionRect> rects, IPoint3D p)
		{
			return rects.Any(rect => Contains(rect.Rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this List<RegionRect> rects, Point3D p)
		{
			return rects.Any(rect => Contains(rect.Rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this List<Rectangle3D> rects, IPoint3D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this List<Rectangle3D> rects, Point3D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this List<Rectangle3D> rects, IPoint2D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y));
		}

		public static bool Contains(this List<Rectangle3D> rects, Point2D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y));
		}

		public static bool Contains(this Rectangle3D rect, IPoint2D p)
		{
			return Contains(rect, p.X, p.Y);
		}

		public static bool Contains(this Rectangle3D rect, IPoint3D p)
		{
			return Contains(rect, p.X, p.Y, p.Z);
		}

		public static bool Contains(this Rectangle3D rect, int x, int y)
		{
			return x >= rect.Start.X && y >= rect.Start.Y && x < rect.End.X && y < rect.End.Y;
		}

		public static bool Contains(this Rectangle3D rect, int x, int y, int z)
		{
			return Contains(rect, x, y) && z >= rect.Start.Z && z < rect.End.Z;
		}

		public static int GetArea(this Rectangle3D r)
		{
			return r.Width * r.Height;
		}

		public static int GetVolume(this Rectangle3D r)
		{
			return r.Width * r.Height * r.Depth;
		}

		public static Rectangle3D Resize(
			this Rectangle3D r,
			int xOffset = 0,
			int yOffset = 0,
			int zOffset = 0,
			int wOffset = 0,
			int hOffset = 0,
			int dOffset = 0)
		{
			var start = r.Start.Clone3D(xOffset, yOffset, zOffset);
			var end = r.End.Clone3D(xOffset + wOffset, yOffset + hOffset, zOffset + dOffset);

			return new Rectangle3D(start, end);
		}

		public static IEnumerable<TEntity> FindEntities<TEntity>(this Rectangle3D r, Map m) where TEntity : IEntity
		{
			if (m == null || m == Map.Internal)
			{
				yield break;
			}

			var o = m.GetObjectsInBounds(r.ToRectangle2D());

			foreach (var e in o.OfType<TEntity>().Where(e => e != null && e.Map == m && r.Contains(e)))
			{
				yield return e;
			}

			o.Free();
		}

		public static IEnumerable<IEntity> FindEntities(this Rectangle3D r, Map m)
		{
			return FindEntities<IEntity>(r, m);
		}

		public static List<TEntity> GetEntities<TEntity>(this Rectangle3D r, Map m) where TEntity : IEntity
		{
			return FindEntities<TEntity>(r, m).ToList();
		}

		public static List<IEntity> GetEntities(this Rectangle3D r, Map m)
		{
			return FindEntities<IEntity>(r, m).ToList();
		}

		public static List<Item> GetItems(this Rectangle3D r, Map m)
		{
			return FindEntities<Item>(r, m).ToList();
		}

		public static List<Mobile> GetMobiles(this Rectangle3D r, Map m)
		{
			return FindEntities<Mobile>(r, m).ToList();
		}

		public static IEnumerable<Point2D> EnumeratePoints2D(this Rectangle3D r)
		{
			for (var x = r.Start.X; x <= r.End.X; x++)
			{
				for (var y = r.Start.Y; y <= r.End.Y; y++)
				{
					yield return new Point2D(x, y);
				}
			}
		}

		public static IEnumerable<Point3D> EnumeratePoints(this Rectangle3D r)
		{
			if (r.Depth > 10)
			{
				Utility.PushColor(ConsoleColor.Yellow);
				"> Warning!".ToConsole();
				"> Rectangle3DExtUtility.EnumeratePoints() called on Rectangle3D with depth exceeding 10;".ToConsole();
				"> This may cause serious performance issues.".ToConsole();
				Utility.PopColor();
			}

			for (var z = r.Start.Z; z <= r.End.Z; z++)
			{
				for (var x = r.Start.X; x <= r.End.X; x++)
				{
					for (var y = r.Start.Y; y <= r.End.Y; y++)
					{
						yield return new Point3D(x, y, z);
					}
				}
			}
		}

		public static void ForEach2D(this Rectangle3D r, Action<Point2D> action)
		{
			if (action == null)
			{
				return;
			}

			foreach (var p in EnumeratePoints2D(r))
			{
				action(p);
			}
		}

		public static void ForEach(this Rectangle3D r, Action<Point3D> action)
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

		public static IEnumerable<Point2D> GetBorder2D(this Rectangle3D r, int size)
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

		public static IEnumerable<Point2D> GetBorder2D(this Rectangle3D r)
		{
			return GetBorder2D(r, 1);
		}

		public static IEnumerable<Point3D> GetBorder(this Rectangle3D r, int size)
		{
			if (r.Depth > 10)
			{
				Utility.PushColor(ConsoleColor.Yellow);
				"> Warning!".ToConsole();
				"> Rectangle3DExtUtility.EnumeratePoints() called on Rectangle3D with depth exceeding 10;".ToConsole();
				"> This may cause serious performance issues.".ToConsole();
				Utility.PopColor();
			}

			size = Math.Max(1, size);

			int x, y;
			int x1 = r.Start.X + size, y1 = r.Start.Y + size, z1 = r.Start.Z+size;
			int x2 = r.End.X - size, y2 = r.End.Y - size,z2=r.End.Z-size;

			for (var z = r.Start.Z; z <= r.End.Z; z++)
			{
				if (z >= z1 || z <= z2)
				{
					continue;
				}

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

						yield return new Point3D(x, y, z);
					}
				}
			}
		}

		public static IEnumerable<Point3D> GetBorder(this Rectangle3D r)
		{
			return GetBorder(r, 1);
		}

		public static bool Intersects2D(this Rectangle3D r, Rectangle3D or)
		{
			return GetBorder2D(r).Any(GetBorder2D(or).Contains);
		}

		public static bool Intersects(this Rectangle3D r, Rectangle3D or)
		{
			var minZL = Math.Min(r.Start.Z, r.End.Z);
			var maxZL = Math.Max(r.Start.Z, r.End.Z);

			var minZR = Math.Min(or.Start.Z, or.End.Z);
			var maxZR = Math.Max(or.Start.Z, or.End.Z);

			if (minZL > maxZR)
			{
				return false;
			}

			if (maxZL < minZR)
			{
				return false;
			}

			return GetBorder2D(r).Union(GetBorder2D(or)).GroupBy(p => p).Any(g => g.Count() > 1);
		}
	}
}