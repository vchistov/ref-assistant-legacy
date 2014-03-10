using System.Collections.Generic;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles
{
    interface ICodeFileHandler
    {
        IEnumerable<TypeId> ResolveReferencedTypes();
    }
}
