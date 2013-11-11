using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using Lardite.RefAssistant.Algorithms.Cache;
using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms.Strategies
{
    internal class TypeInterfacesStrategy : IStrategy<IType>
    {
        private readonly IUsedTypesCache _typesCache;
        private readonly IStrategy<ITypeImport> _importedTypeStrategy;

        public TypeInterfacesStrategy(IUsedTypesCache typesCache)
        {
            Contract.Requires<ArgumentNullException>(typesCache != null);
            _typesCache = typesCache;
            _importedTypeStrategy = new ImportedTypeStrategy(_typesCache);
        }

        public IEnumerable<IAssembly> DoAnalysis(IType inputType)
        {
            if (inputType == null)
            {
                return Enumerable.Empty<IAssembly>();
            }

            var interfaces = inputType.Interfaces.Where(@interface => !_typesCache.IsCached(@interface));

            var assemblies = TakeTypeAssemblies(inputType)
                .Union(interfaces.SelectMany(@interface => 
                    {
                        var interfaceAssemblies = TakeTypeAssemblies(@interface)
                            .Union(_importedTypeStrategy.DoAnalysis(@interface as ITypeImport))
                            .ToList();

                        _typesCache.AddType(@interface);
                        return interfaceAssemblies;
                    }));

            return assemblies;
        }

        /// <summary>
        /// Takes the assemblies of the type. The first one is the assembly where the type is defined, another one is from which the type is forwarded.
        /// </summary>
        private IEnumerable<IAssembly> TakeTypeAssemblies(IType inputType)
        {
            yield return inputType.Assembly;

            if (inputType.ForwardedFrom != null)
            {
                yield return inputType.ForwardedFrom;
            }
        }
    }
}
