using System.Collections.Concurrent;

namespace VitaNex.Collections
{
	public sealed class ConcurrentQueuePool<T> : ObjectPool<ConcurrentQueue<T>>
	{
		public ConcurrentQueuePool()
		{ }

		public ConcurrentQueuePool(int capacity)
			: base(capacity)
		{ }

		protected override bool Sanitize(ConcurrentQueue<T> o)
		{
			while (!o.IsEmpty)
			{
				o.TryDequeue(out _);
			}

			return o.Count == 0;
		}
	}
}
