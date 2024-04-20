namespace System.Collections.Generic
{
	internal sealed class CachedEnumerable<TSource, TResult>(IEnumerable<TSource> source,
															 Func<TSource, TResult> selector)
		: IEnumerable<TResult>
	{
		private readonly object _syncCache = new();
		private Dictionary<TSource, TResult> _cache;

		public IEnumerator<TResult> GetEnumerator()
		{
			if (source == null)
			{
				yield break;
			}

			lock (_syncCache)
			{
				_cache ??= [];
			}

			foreach (TSource key in source)
			{
				TResult item;
				lock (_cache)
				{
					if (!_cache.TryGetValue(key, out item))
					{
						_cache.Add(key, item = selector(key));
					}
				}
				yield return item;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
