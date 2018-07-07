#region Header
//   Vorspire    _,-'/-'/  ObjectPool.cs
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
using System.Collections;
using System.Collections.Generic;
#endregion

namespace VitaNex.Collections
{
	public static class ObjectPool
	{
		public static T Acquire<T>()
			where T : new()
		{
			return ObjectPool<T>.AcquireObject();
		}

		public static void Acquire<T>(out T o)
			where T : new()
		{
			ObjectPool<T>.AcquireObject(out o);
		}

		public static void Free<T>(T o)
			where T : new()
		{
			ObjectPool<T>.FreeObject(o);
		}

		public static void Free<T>(ref T o)
			where T : new()
		{
			ObjectPool<T>.FreeObject(ref o);
		}
	}

	public class ObjectPool<T>
		where T : new()
	{
		private static readonly ObjectPool<T> _Instance;

		static ObjectPool()
		{
			_Instance = new ObjectPool<T>();
		}

		public static T AcquireObject()
		{
			return _Instance.Acquire();
		}

		public static void AcquireObject(out T o)
		{
			o = _Instance.Acquire();
		}

		public static void FreeObject(T o)
		{
			_Instance.Free(o);
		}

		public static void FreeObject(ref T o)
		{
			_Instance.Free(ref o);
		}

		protected readonly Queue<T> _Pool;

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

			try
			{
				if (o is IList)
				{
					var l = (IList)o;

					if (l.Count > 0)
					{
						l.Clear();
					}
				}
				else
				{
					o.CallMethod("Clear");
				}
			}
			catch
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

		public void Free(ref T o)
		{
			Free(o);

			o = default(T);
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