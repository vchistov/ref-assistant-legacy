using System;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface ICustomAttributeArgument
    {
        ITypeDefinition Type { get; }

        object Value { get; }
    }
}
