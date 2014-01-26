using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using Lardite.RefAssistant.Algorithms.Cache;
using Lardite.RefAssistant.Algorithms.Contracts;

namespace Lardite.RefAssistant.Algorithms.Strategies
{
    internal sealed class ClassHierarchyStrategy : IStrategy<ITypeDefinition>
    {
        private readonly IUsedTypesCache _typesCache;

        public ClassHierarchyStrategy(IUsedTypesCache typesCache)
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

            var assemblies = TakeImportedFrom(inputType)                
                .Union(EnumerateHierarchy(inputType)
                .TakeWhile(type => !_typesCache.IsCached(type))
                .Reverse()
                .SelectMany(t =>
                    {
                        _typesCache.AddType(t);
                        return TakeTypeAssemblies(t);
                    }));

            return assemblies;
        }

        /// <summary>
        /// Enumerates all hierarchy, includes the input class.
        /// </summary>
        private IEnumerable<ITypeDefinition> EnumerateHierarchy(ITypeDefinition inputType)
        {
            yield return inputType;

            var baseType = inputType.BaseType;
            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
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
            // Don't join the conditions, to avoid extra request of ImportedFrom property.
            if (_typesCache.IsCached(inputType))
            {
                yield break;
            }

            if (inputType.ImportedFrom != null)
            {
                yield return inputType.ImportedFrom;
            }
        }
    }
}
