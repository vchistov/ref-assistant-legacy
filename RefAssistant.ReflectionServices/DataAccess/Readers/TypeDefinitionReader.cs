using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.ReflectionServices.Data;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Readers
{
    internal sealed class TypeDefinitionReader : ITypeDefinitionReader
    {
        private readonly TypeDefinition _typeDef;
        private readonly ITypeIdProvider _typeIdProvider;

        internal TypeDefinitionReader(TypeDefinition typeDef, ITypeIdProvider typeIdProvider)
        {
            Contract.Requires(typeDef != null);
            Contract.Requires(typeIdProvider != null);

            _typeDef = typeDef;
            _typeIdProvider = typeIdProvider;
        }

        TypeId ITypeDefinitionReader.GetId()
        {
            return _typeIdProvider.GetId(_typeDef);
        }

        TypeId ITypeDefinitionReader.GetBaseType()
        {
            return _typeDef.BaseType != null
                ? _typeIdProvider.GetId(_typeDef.BaseType)
                : null;
        }

        IEnumerable<TypeId> ITypeDefinitionReader.GetInterfaces()
        {
            return _typeDef
                .GetInterfaces()
                .Select(@interface => _typeIdProvider.GetId(@interface))
                .Distinct();
        }

        bool ITypeDefinitionReader.IsInterface
        {
            get
            {
                return _typeDef.IsInterface;
            }
        }
    }
}
