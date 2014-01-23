using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface ICustomAttributeProvider
    {
        IEnumerable<ICustomAttribute> CustomAttributes { get; }
    }
}
