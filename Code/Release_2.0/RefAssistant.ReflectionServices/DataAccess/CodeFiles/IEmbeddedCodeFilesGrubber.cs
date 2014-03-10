using System.Collections.Generic;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles
{
    internal interface IEmbeddedCodeFilesGrubber
    {
        IEnumerable<TypeId> GetReferencedTypes(AssemblyId assemblyId);
    }
}
