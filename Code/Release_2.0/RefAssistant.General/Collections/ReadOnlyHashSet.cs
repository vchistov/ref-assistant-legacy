using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lardite.RefAssistant.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public sealed class ReadOnlyHashSet<T> : IReadOnlyHashSet<T>, ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        const string NotSupportedExceptionMessage = "Collection is read-only.";

        private readonly ISet<T> _set;

        public ReadOnlyHashSet(ISet<T> set)
        {
            ThrowUtils.ArgumentNull(() => set);
            _set = set;
        }

        /// <inheritdoc />
        public int Count
        {
            get { return _set.Count; }
        }

        #region ISet<T>

        /// <inheritdoc />
        bool ISet<T>.Add(T item)
        {
            ThrowUtils.NotSupported(NotSupportedExceptionMessage);
            return false;
        }

        /// <inheritdoc />
        void ISet<T>.ExceptWith(IEnumerable<T> other)
        {
            ThrowUtils.NotSupported(NotSupportedExceptionMessage);
        }

        /// <inheritdoc />
        void ISet<T>.IntersectWith(IEnumerable<T> other)
        {
            ThrowUtils.NotSupported(NotSupportedExceptionMessage);
        }

        /// <inheritdoc />
        bool ISet<T>.IsProperSubsetOf(IEnumerable<T> other)
        {
            return _set.IsProperSubsetOf(other);
        }

        /// <inheritdoc />
        bool ISet<T>.IsProperSupersetOf(IEnumerable<T> other)
        {
            return _set.IsProperSupersetOf(other);
        }

        /// <inheritdoc />
        bool ISet<T>.IsSubsetOf(IEnumerable<T> other)
        {
            return _set.IsSubsetOf(other);
        }

        /// <inheritdoc />
        bool ISet<T>.IsSupersetOf(IEnumerable<T> other)
        {
            return _set.IsSupersetOf(other);
        }

        /// <inheritdoc />
        bool ISet<T>.Overlaps(IEnumerable<T> other)
        {
            return _set.Overlaps(other);
        }

        /// <inheritdoc />
        bool ISet<T>.SetEquals(IEnumerable<T> other)
        {
            return _set.SetEquals(other);
        }

        /// <inheritdoc />
        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
        {
            ThrowUtils.NotSupported(NotSupportedExceptionMessage);
        }

        /// <inheritdoc />
        void ISet<T>.UnionWith(IEnumerable<T> other)
        {
            ThrowUtils.NotSupported(NotSupportedExceptionMessage);
        }

        #endregion

        #region ICollection<T>

        /// <inheritdoc />
        void ICollection<T>.Add(T item)
        {
            ThrowUtils.NotSupported(NotSupportedExceptionMessage);
        }

        /// <inheritdoc />
        void ICollection<T>.Clear()
        {
            ThrowUtils.NotSupported(NotSupportedExceptionMessage);
        }

        /// <inheritdoc />
        bool ICollection<T>.Contains(T item)
        {
            return _set.Contains(item);
        }

        /// <inheritdoc />
        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            _set.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        int ICollection<T>.Count
        {
            get { return _set.Count; }
        }

        /// <inheritdoc />
        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }

        /// <inheritdoc />
        bool ICollection<T>.Remove(T item)
        {
            ThrowUtils.NotSupported(NotSupportedExceptionMessage);
            return false;
        }

        #endregion

        #region IEnumerable<T>

        /// <inheritdoc />
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        #endregion

        #region IEnumerable

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        #endregion
    }
}
