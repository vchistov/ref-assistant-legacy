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
        private readonly ITypeIdResolver _typeIdResolver;

        internal TypeDefinitionReader(TypeDefinition typeDef, ITypeIdResolver typeIdResolver)
        {
            Contract.Requires(typeDef != null);
            Contract.Requires(typeIdResolver != null);

            _typeDef = typeDef;
            _typeIdResolver = typeIdResolver;
        }

        TypeId ITypeDefinitionReader.GetId()
        {
            return _typeIdResolver.GetTypeId(_typeDef);
        }

        TypeId ITypeDefinitionReader.GetBaseType()
        {
            return _typeDef.BaseType != null
                ? _typeIdResolver.GetTypeId(_typeDef.BaseType)
                : null;
        }

        IEnumerable<TypeId> ITypeDefinitionReader.GetInterfaces()
        {
            return _typeDef
                .GetInterfaces()
                .Select(@interface => _typeIdResolver.GetTypeId(@interface))
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
