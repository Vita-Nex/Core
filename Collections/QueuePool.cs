#region Header
//   Vorspire    _,-'/-'/  QueuePool.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;
#endregion

namespace VitaNex.Collections
{
	public sealed class QueuePool<T> : ObjectPool<Queue<T>>
	{
		public QueuePool()
			: this(32)
		{ }

		public QueuePool(int capacity)
			: base(capacity)
		{ }

		public override void Free(Queue<T> o)
		{
			if (o == null)
			{
				return;
			}

			o.Clear();

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