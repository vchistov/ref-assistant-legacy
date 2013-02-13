//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Cache of the typed objects.
    /// </summary>
    /// <typeparam name="TKey">Type of the Keys.</typeparam>
    /// <typeparam name="TValue">Type of the Values.</typeparam>
    public abstract class SimpleCache<TKey, TValue>
    {
        #region Fields

        private readonly Dictionary<TKey, WeakReference> cache = new Dictionary<TKey, WeakReference>();

        #endregion

        #region Properties

        /// <summary>
        /// Get the cached value by the key.
        /// </summary>
        /// <param name="key">The key of the value.</param>
        /// <returns>Return cached value.</returns>
        public TValue this[TKey key]
        {
            get
            {
                if (!cache.ContainsKey(key))
                {
                    return RegenerateValue(key);
                }
                return GetCachedValue(key);
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Remove the cached value.
        /// </summary>
        /// <param name="key">The key of the value.</param>
        public void Remove(TKey key)
        {
            if (cache.ContainsKey(key))
            {
                cache.Remove(key);
            }
        }

        /// <summary>
        /// Clear cache.
        /// </summary>
        public void Expire()
        {
            cache.Clear();
        }

        #endregion

        #region Abstract methods

        /// <summary>
        /// Get the new value by the key.
        /// </summary>
        /// <param name="key">The key of the necessary value.</param>
        /// <returns></returns>
        protected abstract TValue GetValue(TKey key);

        #endregion

        #region Private methods

        private TValue GetCachedValue(TKey key)
        {
            var value = cache[key].Target;
            if (value == null)
            {
                cache.Remove(key);
                return RegenerateValue(key);
            }
            return (TValue)value;
        }

        private TValue RegenerateValue(TKey key)
        {
            var value = GetValue(key);
            cache.Add(key, new WeakReference(value, false));
            return value;
        }

        #endregion
    }
}
