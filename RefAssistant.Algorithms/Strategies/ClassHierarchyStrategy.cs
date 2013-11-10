using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using Lardite.RefAssistant.Algorithms.Cache;
using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms.Strategies
{
    internal sealed class ClassHierarchyStrategy : IStrategy
    {
        private readonly IUsedTypesCache _typesCache;

        public ClassHierarchyStrategy(IUsedTypesCache typesCache)
        {
            Contract.Requires<ArgumentNullException>(typesCache != null);
            _typesCache = typesCache;
        }

        public IEnumerable<IAssembly> DoAnalysis(IType inputType)
        {
            if (inputType == null)
            {
                return Enumerable.Empty<IAssembly>();
            }

            var assemblies = EnumerateHierarchy(inputType)
                .TakeWhile(type => !_typesCache.IsCached(type))
                .Reverse()
                .SelectMany(t =>
                    {
                        _typesCache.AddType(t);
                        return TakeTypeAssemblies(t);
                    })
                .Distinct();

            return assemblies;
        }

        /// <summary>
        /// Enumerates all hierarchy, includes the input class.
        /// </summary>
        private IEnumerable<IType> EnumerateHierarchy(IType inputType)
        {
            IType baseType = null;
            if (inputType != null)
            {
                baseType = inputType.BaseType;
                yield return inputType;
            }

            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
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
