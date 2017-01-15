#region Header
//   Vorspire    _,-'/-'/  ObjectPool.cs
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
#endregion

namespace VitaNex.Collections
{
	public class ObjectPool<T>
		where T : new()
	{
		private readonly Queue<T> _Pool;

		public int Capacity { get; set; }

		public ObjectPool()
			: this(32)
		{ }

		public ObjectPool(int capacity)
		{
			_Pool = new Queue<T>(capacity);
		}

		public virtual T Acquire()
		{
			lock (_Pool)
			{
				if (_Pool.Count > 0)
				{
					return _Pool.Dequeue();
				}
			}

			return new T();
		}

		public virtual void Free(T o)
		{
			if (o == null)
			{
				return;
			}

			lock (_Pool)
			{
				if (_Pool.Count < Capacity)
				{
					_Pool.Enqueue(o);
				}
			}
		}

		public virtual int Trim()
		{
			lock (_Pool)
			{
				var c = 0;

				while (_Pool.Count > Capacity)
				{
					_Pool.Dequeue();

					++c;
				}

				return c;
			}
		}

		public virtual int Fill()
		{
			lock (_Pool)
			{
				var c = 0;

				while (_Pool.Count < Capacity)
				{
					_Pool.Enqueue(new T());

					++c;
				}

				return c;
			}
		}

		public virtual void Free()
		{
			lock (_Pool)
			{
				_Pool.Free(true);
			}
		}
	}
}