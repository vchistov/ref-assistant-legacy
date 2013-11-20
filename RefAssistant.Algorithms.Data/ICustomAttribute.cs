using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface ICustomAttribute : IEquatable<ICustomAttribute>
    {
        ITypeDefinition AttributeType { get; }

        IEnumerable<ICustomAttributeArgument> ConstructorArguments { get; }

        IEnumerable<ICustomAttributeArgument> Fields { get; }

        IEnumerable<ICustomAttributeArgument> Properties { get; }
    }
}
