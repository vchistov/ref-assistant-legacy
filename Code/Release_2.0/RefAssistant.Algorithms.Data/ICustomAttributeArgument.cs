using System;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface ICustomAttributeArgument : IEquatable<ICustomAttributeArgument>
    {
        ITypeDefinition Type { get; }

        object Value { get; }
    }
}
