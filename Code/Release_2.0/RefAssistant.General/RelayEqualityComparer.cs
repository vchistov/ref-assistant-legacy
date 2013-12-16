using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Lardite.RefAssistant
{
    public sealed class RelayEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equals;
        private readonly Func<T, int> _getHashCode;

        public RelayEqualityComparer(Func<T, T, bool> equals)
            : this(equals, GetHashCodeDefault)
        {
        }

        public RelayEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            ThrowUtils.ArgumentNull(() => equals);
            ThrowUtils.ArgumentNull(() => getHashCode);

            _equals = equals;
            _getHashCode = getHashCode;
        }

        public bool Equals(T x, T y)
        {
            Contract.Assert(_equals != null);
            return _equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            Contract.Assert(_getHashCode != null);
            return _getHashCode(obj);
        }

        private static int GetHashCodeDefault(T obj)
        {
            return obj != null ? obj.GetHashCode() : 0;
        }
    }
}
