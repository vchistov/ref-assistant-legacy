using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface ITypeReference
    {
        ITypeDefinition TypeDefinition { get; }

        IEnumerable<IMember> MemberReferences { get; }
    }
}
