using Lardite.RefAssistant.ReflectionServices.Data;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess
{
    internal interface ITypeIdProvider
    {
        TypeId GetId(TypeReference typeRef);

        TypeId GetId(TypeDefinition typeDef);
    }
}
