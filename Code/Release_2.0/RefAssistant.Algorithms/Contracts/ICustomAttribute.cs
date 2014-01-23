using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface ICustomAttribute
    {
        ITypeDefinition AttributeType { get; }

        IEnumerable<ITypeDefinition> ConstructorArguments { get; }

        IEnumerable<ITypeDefinition> Fields { get; }

        IEnumerable<ITypeDefinition> Properties { get; }
    }
}
