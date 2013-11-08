using System.Collections.Generic;
using System.Threading;

using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms.Cache
{
    internal sealed class UsedTypesCache : Singleton<UsedTypesCache>, IUsedTypesCache
    {
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly HashSet<IType> _usedTypes = new HashSet<IType>();

        private UsedTypesCache()
        { }

        public bool IsCached(IType typeInfo)
        {
            _locker.EnterReadLock();
            try
            {
                return _usedTypes.Contains(typeInfo);
            }
            finally
            {
                _locker.ExitReadLock();
            }            
        }

        public void AddType(IType typeInfo)
        {
            _locker.EnterWriteLock();
            try
            {
                _usedTypes.Add(typeInfo);
            }
            finally
            {
                _locker.ExitWriteLock();
            }            
        }
    }
}
