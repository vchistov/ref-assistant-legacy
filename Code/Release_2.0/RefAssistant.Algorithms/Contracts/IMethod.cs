﻿using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface IMethod : IMember
    {
        IMemberType ReturnType { get; }
        
        IEnumerable<IMemberParameter> Parameters { get; }
    }
}
