using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using Lardite.RefAssistant.Algorithms.Cache;
using Lardite.RefAssistant.Algorithms.Contracts;

namespace Lardite.RefAssistant.Algorithms.Strategies
{
    internal class TypeInterfacesStrategy : IStrategy<ITypeDefinition>
    {
        private readonly IUsedTypesCache _typesCache;

        public TypeInterfacesStrategy(IUsedTypesCache typesCache)
        {
            Contract.Requires<ArgumentNullException>(typesCache != null);
            _typesCache = typesCache;
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
                        Contract.Assert(@interface.IsInterface);

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

        /// <summary>
        /// Takes the assembly from which the type is imported.
        /// </summary>
        private IEnumerable<IAssembly> TakeImportedFrom(ITypeDefinition inputType)
        {
            if (inputType.ImportedFrom != null)
            {
                yield return inputType.ImportedFrom;
            }            
        }
    }
}
