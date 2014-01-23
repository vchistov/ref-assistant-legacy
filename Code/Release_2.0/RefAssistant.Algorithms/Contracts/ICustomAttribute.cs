using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface ICustomAttribute
    {
        ITypeDefinition AttributeType { get; }

        IEnumerable<ICustomAttributeArgument> ConstructorArguments { get; }

        IEnumerable<ICustomAttributeArgument> Fields { get; }

        IEnumerable<ICustomAttributeArgument> Properties { get; }
    }
}
