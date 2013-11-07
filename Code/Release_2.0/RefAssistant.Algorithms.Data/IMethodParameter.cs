using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface IMethodParameter : ICustomAttributeProvider, IEquatable<IMethodParameter>
    {
        IType Type { get; }
    }
}
