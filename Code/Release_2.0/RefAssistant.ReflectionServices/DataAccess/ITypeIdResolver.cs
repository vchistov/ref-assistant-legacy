using Lardite.RefAssistant.ReflectionServices.Data;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess
{
    internal interface ITypeIdResolver
    {
        TypeId GetTypeId(TypeReference typeRef);

        TypeId GetTypeId(TypeDefinition typeDef);
    }
}
