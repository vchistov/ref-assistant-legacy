using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface ITypeReference : IEquatable<ITypeReference>
    {
        ITypeDefinition TypeDefinition { get; }

        IEnumerable<IMember> MemberReferences { get; }
    }
}
