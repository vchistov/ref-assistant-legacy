using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant
{
    public sealed class GenericCache<TKey, TValue>
        where TKey : IEquatable<TKey>
    {
        private readonly IDictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();

        public TValue Get(TKey key, Func<TKey, TValue> valueResolver)
        {
            TValue value = default(TValue);
            if (!_cache.TryGetValue(key, out value))
            {
                value = valueResolver(key);
                _cache.Add(key, value);
            }
            return value;
        }
    }
}
