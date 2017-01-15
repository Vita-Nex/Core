#region Header
//   Vorspire    _,-'/-'/  ListPool.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;
#endregion

namespace VitaNex.Collections
{
	public sealed class ListPool<T> : ObjectPool<List<T>>
	{
		public ListPool()
			: this(32)
		{ }

		public ListPool(int capacity)
			: base(capacity)
		{ }

		public override void Free(List<T> o)
		{
			if (o != null)
			{
				o.Clear();
			}

			base.Free(o);
		}
	}
}