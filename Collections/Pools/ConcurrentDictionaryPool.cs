using System.Collections.Concurrent;

namespace VitaNex.Collections
{
	public sealed class ConcurrentDictionaryPool<TKey, TVal> : ObjectPool<ConcurrentDictionary<TKey, TVal>>
	{
		public ConcurrentDictionaryPool()
		{ }

		public ConcurrentDictionaryPool(int capacity)
			: base(capacity)
		{ }

		protected override bool Sanitize(ConcurrentDictionary<TKey, TVal> o)
		{
			o.Clear();

			return o.Count == 0;
		}
	}
}
