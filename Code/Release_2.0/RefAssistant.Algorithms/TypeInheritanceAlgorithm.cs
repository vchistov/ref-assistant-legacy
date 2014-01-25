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
        private readonly IStrategy<ITypeImport> _importedTypeStrategy;

        public TypeInheritanceAlgorithm()
            : this(
                new ClassHierarchyStrategy(UsedTypesCache.Instance),
                new TypeInterfacesStrategy(UsedTypesCache.Instance),
                new ImportedTypeStrategy(UsedTypesCache.Instance))
        {
        }

        internal TypeInheritanceAlgorithm(
            IStrategy<ITypeDefinition> classHierarchyStrategy,
            IStrategy<ITypeDefinition> typeInterfacesStrategy,
            IStrategy<ITypeImport> importedTypeStrategy)
        {
            Contract.Requires(classHierarchyStrategy != null);
            Contract.Requires(typeInterfacesStrategy != null);
            Contract.Requires(importedTypeStrategy != null);

            _classHierarchyStrategy = classHierarchyStrategy;
            _typeInterfacesStrategy = typeInterfacesStrategy;
            _importedTypeStrategy = importedTypeStrategy;
        }

        public AlgorithmResult Process(IEnumerable<ITypeDefinition> input)
        {
            var requiredAssemblies = new HashSet<IAssembly>();

            foreach (var type in input)
            {
                requiredAssemblies
                    .UnionWithFluent(_classHierarchyStrategy.DoAnalysis(type))
                    .UnionWithFluent(_typeInterfacesStrategy.DoAnalysis(type))
                    .UnionWithFluent(_importedTypeStrategy.DoAnalysis(type as ITypeImport));
            }

            return new AlgorithmResult(requiredAssemblies, this.GetType().FullName);
        }
    }
}
