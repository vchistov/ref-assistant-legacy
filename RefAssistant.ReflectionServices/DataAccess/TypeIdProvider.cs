using System.Diagnostics.Contracts;
using Lardite.RefAssistant.ReflectionServices.Data;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess
{
    internal sealed class TypeIdProvider : Singleton<TypeIdProvider>, ITypeIdProvider
    {
        private TypeIdProvider() { }

        public TypeId GetId(TypeReference typeRef)
        {
            IMetadataScope forwardedFromScope;
            var typeDef = typeRef.Resolve(out forwardedFromScope);

            var forwardedFromId = forwardedFromScope == null
                ? (AssemblyId)null
                : AssemblyId.GetId(forwardedFromScope.GetAssemblyNameReference().FullName);

            return GetId(typeDef, forwardedFromId);
        }

        public TypeId GetId(TypeDefinition typeDef)
        {
            return GetId(typeDef, null);
        }

        #region Helpers

        private static TypeId GetId(TypeDefinition typeDef, AssemblyId forwardedFromId)
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
