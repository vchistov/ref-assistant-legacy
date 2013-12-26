using System.Collections.Generic;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.ReflectionServices
{
    public interface ICustomAttributeService
    {
        IEnumerable<CustomAttributeInfo> GetAssemblyAttributes(AssemblyId assemblyId);

        IEnumerable<CustomAttributeInfo> GetTypeAttributes(TypeId typeId);
    }
}
