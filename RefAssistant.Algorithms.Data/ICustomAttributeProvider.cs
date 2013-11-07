using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface ICustomAttributeProvider
    {
        IEnumerable<ICustomAttribute> CustomAttributes { get; }
    }
}
