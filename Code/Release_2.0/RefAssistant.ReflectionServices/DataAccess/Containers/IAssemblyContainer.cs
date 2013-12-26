using Lardite.RefAssistant.ReflectionServices.Data;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Containers
{
    internal interface IAssemblyContainer
    {
        AssemblyDefinition Get(AssemblyId assemblyId);
    }
}
