using System.Collections.Concurrent;

namespace VitaNex.Collections
{
	public sealed class ConcurrentBagPool<T> : ObjectPool<ConcurrentBag<T>>
	{
		public ConcurrentBagPool()
		{ }

		public ConcurrentBagPool(int capacity)
			: base(capacity)
		{ }

		protected override bool Sanitize(ConcurrentBag<T> o)
		{
			while (!o.IsEmpty)
			{
				o.TryTake(out _);
			}

			return o.Count == 0;
		}
	}
}
