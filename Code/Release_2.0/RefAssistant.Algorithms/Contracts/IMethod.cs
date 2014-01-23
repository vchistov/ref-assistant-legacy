using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface IMethod : IMember
    {
        ITypeDefinition ReturnType { get; }

        IEnumerable<IMethodParameter> Parameters { get; }
    }
}
