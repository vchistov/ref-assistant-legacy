using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using Lardite.RefAssistant.Algorithms.Cache;
using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms.Strategies
{
    internal class TypeInterfacesStrategy : IStrategy<ITypeDefinition>
    {
        private readonly IUsedTypesCache _typesCache;
        private readonly IStrategy<ITypeImport> _importedTypeStrategy;

        public TypeInterfacesStrategy(IUsedTypesCache typesCache)
        {
            Contract.Requires<ArgumentNullException>(typesCache != null);
            _typesCache = typesCache;
            _importedTypeStrategy = new ImportedTypeStrategy(_typesCache);
        }

        public IEnumerable<IAssembly> DoAnalysis(ITypeDefinition inputType)
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
                            .Union(TakeImportedFrom(@interface))
                            .ToList();

                        _typesCache.AddType(@interface);
                        return interfaceAssemblies;
                    }));

            return assemblies;
        }

        /// <summary>
        /// Takes the assemblies of the type. The first one is the assembly where the type is defined, another one is from which the type is forwarded.
        /// </summary>
        private IEnumerable<IAssembly> TakeTypeAssemblies(ITypeDefinition inputType)
        {
            yield return inputType.Assembly;

            if (inputType.ForwardedFrom != null)
            {
                yield return inputType.ForwardedFrom;
            }
        }

        private IEnumerable<IAssembly> TakeImportedFrom(ITypeDefinition inputType)
        {
            var typeImport = inputType as ITypeImport;
            if (typeImport == null || typeImport.ImportedFrom == null)
            {
                yield break;
            }

            foreach (var @assembly in _importedTypeStrategy.DoAnalysis(typeImport))
            {
                yield return @assembly;
            }
        }
    }
}
