using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface IMethodParameter : ICustomAttributeProvider
    {
        ITypeDefinition Type { get; }
    }
}
