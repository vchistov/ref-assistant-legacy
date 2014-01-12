using System.Diagnostics.Contracts;
using Lardite.RefAssistant.ReflectionServices.Data;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess
{
    internal sealed class TypeIdResolver : Singleton<TypeIdResolver>, ITypeIdResolver
    {
        private TypeIdResolver() { }

        public TypeId GetTypeId(TypeReference typeRef)
        {
            IMetadataScope forwardedFromScope;
            var typeDef = typeRef.Resolve(out forwardedFromScope);

            var forwardedFromId = forwardedFromScope == null
                ? (AssemblyId)null
                : AssemblyId.GetId(forwardedFromScope.GetAssemblyNameReference().FullName);

            return GetTypeId(typeDef, forwardedFromId);
        }

        public TypeId GetTypeId(TypeDefinition typeDef)
        {
            return GetTypeId(typeDef, null);
        }

        #region Helpers

        private static TypeId GetTypeId(TypeDefinition typeDef, AssemblyId forwardedFromId)
        {
            Contract.Requires(typeDef != null);
            Contract.Ensures(Contract.Result<TypeId>() != null);

            var assemblyId = AssemblyId.GetId(typeDef.Scope.GetAssemblyNameReference().FullName);
            var typeId = TypeId.GetId(typeDef.FullName, assemblyId, forwardedFromId);

            return typeId;
        }

        #endregion
    }
}
