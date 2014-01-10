using Lardite.RefAssistant.ReflectionServices.Data;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Lookups
{
    internal interface ITypeLookup
    {
        TypeDefinition Get(TypeId typeId);
    }
}
