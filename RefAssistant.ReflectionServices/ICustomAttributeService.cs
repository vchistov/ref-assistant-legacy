using System.Collections.Generic;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.ReflectionServices
{
    public interface ICustomAttributeService
    {
        IEnumerable<CustomAttributeInfo> GetAttributes(AssemblyId assemblyId);

        IEnumerable<CustomAttributeInfo> GetAttributes(TypeId typeId);
    }
}
