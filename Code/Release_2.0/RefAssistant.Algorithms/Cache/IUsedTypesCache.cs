using System;
using Lardite.RefAssistant.Algorithms.Contracts;

namespace Lardite.RefAssistant.Algorithms.Cache
{
    internal interface IUsedTypesCache
    {
        bool IsCached(ITypeDefinition typeInfo);

        void AddType(ITypeDefinition typeInfo);
    }
}
