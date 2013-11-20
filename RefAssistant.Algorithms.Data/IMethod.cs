using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface IMethod : IMember, IEquatable<IMethod>
    {
        ITypeDefinition ReturnType { get; }

        IEnumerable<IMethodParameter> Parameters { get; }
    }
}
