using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface ITypeDefinition : ICustomAttributeProvider, IEquatable<ITypeDefinition>
    {
        TypeName Name { get; }

        ITypeDefinition BaseType { get; }

        IEnumerable<ITypeDefinition> Interfaces { get; }

        IAssembly Assembly { get; }

        IAssembly ForwardedFrom { get; }

        bool IsInterface { get; }

        IEnumerable<IMethod> Methods { get; }

        IEnumerable<IMember> Fields { get; }

        IEnumerable<IMember> Properties { get; }

        IEnumerable<IMember> Events { get; }
    }
}
