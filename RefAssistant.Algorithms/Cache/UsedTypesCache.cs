using System.Collections.Generic;
using System.Threading;

using Lardite.RefAssistant.Algorithms.Contracts;

namespace Lardite.RefAssistant.Algorithms.Cache
{
    internal sealed class UsedTypesCache : Singleton<UsedTypesCache>, IUsedTypesCache
    {
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly HashSet<ITypeDefinition> _usedTypes = new HashSet<ITypeDefinition>();

        public bool IsCached(ITypeDefinition typeInfo)
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

        public void AddType(ITypeDefinition typeInfo)
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
