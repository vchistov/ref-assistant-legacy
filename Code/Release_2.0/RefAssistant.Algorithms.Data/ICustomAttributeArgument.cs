using System;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface ICustomAttributeArgument : IEquatable<ICustomAttributeArgument>
    {
        IType Type { get; }

        object Value { get; }
    }
}
