using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface IMemberType
    {
        bool IsGenericParameter { get; }

        ITypeDefinition TypeDefinition { get; }

        IEnumerable<IMemberType> GenericArguments { get; }
    }
}
