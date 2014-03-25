using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Cache;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.Algorithms.Strategies;
using Lardite.RefAssistant.Collections;

namespace Lardite.RefAssistant.Algorithms
{
    public sealed class ReferencedTypeAlgorithm : IAlgorithm<IEnumerable<ITypeReference>>
    {
        private readonly IStrategy<ITypeDefinition> _classHierarchyStrategy;
        private readonly IStrategy<ITypeDefinition> _typeInterfacesStrategy;
        private readonly IUsedTypesCache _typesCache;

        public ReferencedTypeAlgorithm()
            : this(
                new ClassHierarchyStrategy(UsedTypesCache.Instance),
                new TypeInterfacesStrategy(UsedTypesCache.Instance),
                UsedTypesCache.Instance)
        {
        }

        internal ReferencedTypeAlgorithm(
            IStrategy<ITypeDefinition> classHierarchyStrategy,
            IStrategy<ITypeDefinition> typeInterfacesStrategy,
            IUsedTypesCache typesCache)
        {
            Contract.Requires(classHierarchyStrategy != null);
            Contract.Requires(typeInterfacesStrategy != null);
            Contract.Requires(typesCache != null);

            _classHierarchyStrategy = classHierarchyStrategy;
            _typeInterfacesStrategy = typeInterfacesStrategy;
            _typesCache = typesCache;
        }

        public AlgorithmResult Process(IEnumerable<ITypeReference> input)
        {
            Contract.Requires(input != null);

            var requiredAssemblies = new HashSet<IAssembly>();

            foreach (var type in input)
            {
                requiredAssemblies
                    .UnionWithFluent(ProcessTypeDefinition(type.TypeDefinition))
                    .UnionWithFluent(ProcessMethodReferences(type.TypeDefinition, type.MethodReferences))
                    .UnionWithFluent(ProcessPropertyReferences(type.PropertyReferences))
                    .UnionWithFluent(ProcessFieldReferences(type.FieldReferences))
                    .UnionWithFluent(ProcessEventReferences(type.EventReferences));
            }

            return new AlgorithmResult(requiredAssemblies, this.GetType().FullName);
        }

        #region Helpers

        private IEnumerable<IAssembly> ProcessTypeDefinition(ITypeDefinition typeDef)
        {
            if (_typesCache.IsCached(typeDef))
            {
                return Enumerable.Empty<IAssembly>();
            }

            return _classHierarchyStrategy.DoAnalysis(typeDef)
                .Concat(_typeInterfacesStrategy.DoAnalysis(typeDef));
        }

        private IEnumerable<IAssembly> ProcessMethodReferences(ITypeDefinition typeDef, IEnumerable<IMethod> methods)
        {
            return methods.AsNotNull()
                .SelectMany(method => MethodTypesSelector(method, typeDef))
                .SelectMany(ProcessMemberType);
        }

        private IEnumerable<IAssembly> ProcessPropertyReferences(IEnumerable<IProperty> properties)
        {
            return properties.AsNotNull()
                .SelectMany(PropertyTypesSelector)
                .SelectMany(ProcessMemberType);
        }

        private IEnumerable<IAssembly> ProcessFieldReferences(IEnumerable<IField> fields)
        {
            return fields.AsNotNull().SelectMany(field => ProcessMemberType(field.FieldType));
        }

        private IEnumerable<IAssembly> ProcessEventReferences(IEnumerable<IEvent> events)
        {
            return events.AsNotNull().SelectMany(@event => ProcessMemberType(@event.EventType));
        }

        private IEnumerable<IAssembly> ProcessMemberType(IMemberType memberType)
        {
            if (memberType.IsGenericParameter)
            {
                yield break;
            }

            if (memberType.TypeDefinition != null)
            {
                foreach (var assembly in ProcessTypeDefinition(memberType.TypeDefinition))
                {
                    yield return assembly;
                }
            }

            foreach (var assembly in memberType.GenericArguments.AsNotNull().SelectMany(mt => ProcessMemberType(mt)))
            {
                yield return assembly;
            }
        }

        private IEnumerable<IMemberType> PropertyTypesSelector(IProperty property)
        {
            yield return property.PropertyType;
            foreach (var @params in property.Parameters.AsNotNull())
            {
                yield return @params.ParameterType;
            }
        }

        private IEnumerable<IMemberType> MethodTypesSelector(IMethod method, ITypeDefinition typeDef)
        {
            foreach (var overloaded in typeDef.GetMethodWithOverloads(method))
            {
                yield return overloaded.ReturnType;
                foreach (var @params in overloaded.Parameters.AsNotNull())
                {
                    yield return @params.ParameterType;
                }
            }
        }

        #endregion
    }
}
