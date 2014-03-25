using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface ITypeDefinition : ICustomAttributeProvider
    {
        TypeName Name { get; }

        ITypeDefinition BaseType { get; }

        IEnumerable<ITypeDefinition> Interfaces { get; }

        IAssembly Assembly { get; }

        IAssembly ForwardedFrom { get; }

        IAssembly ImportedFrom { get; }

        bool IsInterface { get; }

        IEnumerable<IMethod> Methods { get; }

        IEnumerable<IField> Fields { get; }

        IEnumerable<IProperty> Properties { get; }

        IEnumerable<IEvent> Events { get; }
    }
}
