using System.Collections.Generic;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Readers
{
    internal interface ICustomAttributeReader
    {
        TypeId GetAttributeType();

        IEnumerable<TypeId> GetConstructorArguments();

        IEnumerable<TypeId> GetFields();

        IEnumerable<TypeId> GetProperties();
    }
}
