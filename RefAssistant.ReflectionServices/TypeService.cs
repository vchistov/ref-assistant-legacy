using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.ReflectionServices.Data;
using Lardite.RefAssistant.ReflectionServices.DataAccess;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Containers;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Lookups;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Readers;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices
{
    internal class TypeService : ITypeService
    {
        private readonly IAssemblyContainer _container;
        private readonly ITypeLookup _typeLookup;

        internal TypeService(IAssemblyContainer container)
        {
            Contract.Requires(container != null);

            _container = container;
            _typeLookup = new TypeLookup(container);
        }

        TypeInfo ITypeService.GetType(TypeId typeId)
        {
            ThrowUtils.ArgumentNull(() => typeId);

            return GetTypeInfo(typeId);
        }

        IEnumerable<TypeInfo> ITypeService.GetInterfaces(TypeId typeId)
        {
            ThrowUtils.ArgumentNull(() => typeId);

            var typeDef = _typeLookup.Get(typeId);
            var reader = CreateReader(typeDef);

            return reader.GetInterfaces().Select(GetTypeInfo);
        }

        #region Helpers

        private TypeInfo GetTypeInfo(TypeId typeId)
        {
            Contract.Requires(typeId != null);
            Contract.Ensures(Contract.Result<TypeInfo>() != null);

            var typeDef = _typeLookup.Get(typeId);
            var reader = CreateReader(typeDef);

            return new TypeInfo(reader);
        }

        private ITypeDefinitionReader CreateReader(TypeDefinition typeDef)
        {
            Contract.Requires(typeDef != null);
            Contract.Ensures(Contract.Result<ITypeDefinitionReader>() != null);

            return new TypeDefinitionReader(typeDef, TypeIdResolver.Instance);
        }

        #endregion
    }
}
