using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Readers
{
    internal interface ITypeDefinitionReader
    {
        TypeId GetId();

        TypeId GetBaseType();

        IEnumerable<TypeId> GetInterfaces();

        bool IsInterface { get; }
    }
}
