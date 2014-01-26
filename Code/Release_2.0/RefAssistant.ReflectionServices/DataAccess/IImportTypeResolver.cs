using Lardite.RefAssistant.ReflectionServices.Data;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess
{
    internal interface IImportTypeResolver
    {
        bool IsImport(TypeId typeId);

        TypeDefinition Resolve(TypeId typeId);
    }
}
