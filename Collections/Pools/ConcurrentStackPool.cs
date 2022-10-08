using System.Collections.Concurrent;

namespace VitaNex.Collections
{
	public sealed class ConcurrentStackPool<T> : ObjectPool<ConcurrentStack<T>>
	{
		public ConcurrentStackPool()
		{ }

		public ConcurrentStackPool(int capacity)
			: base(capacity)
		{ }

		protected override bool Sanitize(ConcurrentStack<T> o)
		{
			o.Clear();

			return o.Count == 0;
		}
	}
}
