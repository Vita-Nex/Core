#region Header
//   Vorspire    _,-'/-'/  GridPool.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
#endregion

namespace VitaNex.Collections
{
	public sealed class GridPool<T> : ObjectPool<Grid<T>>
	{
		public GridPool()
			: this(32)
		{ }

		public GridPool(int capacity)
			: base(capacity)
		{ }

		public override void Free(Grid<T> o)
		{
			if (o == null)
			{
				return;
			}

			o.Resize(0, 0);

			lock (_Pool)
			{
				if (_Pool.Count < Capacity)
				{
					_Pool.Enqueue(o);
				}
			}
		}
	}
}