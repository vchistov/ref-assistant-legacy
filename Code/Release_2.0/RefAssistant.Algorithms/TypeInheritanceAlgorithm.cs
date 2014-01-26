using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Lardite.RefAssistant.Algorithms.Cache;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.Algorithms.Strategies;
using Lardite.RefAssistant.Collections;

namespace Lardite.RefAssistant.Algorithms
{
    public sealed class TypeInheritanceAlgorithm : IAlgorithm<IEnumerable<ITypeDefinition>>
    {
        private readonly IStrategy<ITypeDefinition> _classHierarchyStrategy;
        private readonly IStrategy<ITypeDefinition> _typeInterfacesStrategy;

        public TypeInheritanceAlgorithm()
            : this(
                new ClassHierarchyStrategy(UsedTypesCache.Instance),
                new TypeInterfacesStrategy(UsedTypesCache.Instance))
        {
        }

        internal TypeInheritanceAlgorithm(
            IStrategy<ITypeDefinition> classHierarchyStrategy,
            IStrategy<ITypeDefinition> typeInterfacesStrategy)
        {
            Contract.Requires(classHierarchyStrategy != null);
            Contract.Requires(typeInterfacesStrategy != null);

            _classHierarchyStrategy = classHierarchyStrategy;
            _typeInterfacesStrategy = typeInterfacesStrategy;
        }

        public AlgorithmResult Process(IEnumerable<ITypeDefinition> input)
        {
            var requiredAssemblies = new HashSet<IAssembly>();

            foreach (var type in input)
            {
                requiredAssemblies
                    .UnionWithFluent(_classHierarchyStrategy.DoAnalysis(type))
                    .UnionWithFluent(_typeInterfacesStrategy.DoAnalysis(type));
            }

            return new AlgorithmResult(requiredAssemblies, this.GetType().FullName);
        }
    }
}
