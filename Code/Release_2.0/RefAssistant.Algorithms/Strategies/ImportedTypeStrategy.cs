using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Lardite.RefAssistant.Algorithms.Cache;
using Lardite.RefAssistant.Algorithms.Contracts;

namespace Lardite.RefAssistant.Algorithms.Strategies
{
    internal sealed class ImportedTypeStrategy : IStrategy<ITypeImport>
    {
        private readonly IUsedTypesCache _typesCache;

        public ImportedTypeStrategy(IUsedTypesCache typesCache)
        {
            Contract.Requires<ArgumentNullException>(typesCache != null);
            _typesCache = typesCache;
        }

        public IEnumerable<IAssembly> DoAnalysis(ITypeImport inputType)
        {
            if (inputType == null || _typesCache.IsCached(inputType))
            {
                yield break;
            }

            Contract.Assert(inputType.ImportedFrom != null);

            _typesCache.AddType(inputType);
            yield return inputType.ImportedFrom;
        }
    }
}
