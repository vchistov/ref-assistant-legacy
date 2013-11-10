using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface IType : ICustomAttributeProvider, IEquatable<IType>
    {
        TypeName Name { get; }

        IType BaseType { get; }

        IEnumerable<IType> Interfaces { get; }

        IAssembly Assembly { get; }

        IAssembly ForwardedFrom { get; }

        bool IsInterface { get; }

        bool IsImport { get; }
    }
}
