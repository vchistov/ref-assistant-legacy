using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.ReflectionServices
{
    public interface ITypeService
    {
        TypeInfo GetType(TypeId typeId);        

        IEnumerable<TypeInfo> GetInterfaces(TypeId typeId);

        AssemblyId GetImportedFrom(TypeId typeId);
    }
}
