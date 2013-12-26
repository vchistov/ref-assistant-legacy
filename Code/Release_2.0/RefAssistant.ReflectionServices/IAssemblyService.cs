using System.Collections.Generic;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.ReflectionServices
{
    public interface IAssemblyService
    {
        AssemblyInfo GetProjectAssembly();

        AssemblyInfo GetAssembly(AssemblyId assemblyId);

        IEnumerable<AssemblyId> GetManifestAssemblies(AssemblyId assemblyId);
    }
}
