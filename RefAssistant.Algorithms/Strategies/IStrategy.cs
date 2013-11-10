using System.Collections.Generic;
using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms.Strategies
{
    internal interface IStrategy<T> where T : IType
    {
        IEnumerable<IAssembly> DoAnalysis(T inputType);
    }
}
