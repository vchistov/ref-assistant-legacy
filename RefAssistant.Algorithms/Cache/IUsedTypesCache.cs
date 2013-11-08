using System;
using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms.Cache
{
    internal interface IUsedTypesCache
    {
        bool IsCached(IType typeInfo);

        void AddType(IType typeInfo);
    }
}
