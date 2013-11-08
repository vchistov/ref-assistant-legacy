using System.Collections.Generic;
using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms.Strategies
{
    internal interface IStrategy
    {
        IEnumerable<IAssembly> DoAnalysis(IType type);
    }
}
