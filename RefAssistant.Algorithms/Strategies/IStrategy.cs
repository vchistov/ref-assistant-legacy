using System.Collections.Generic;
using Lardite.RefAssistant.Algorithms.Contracts;

namespace Lardite.RefAssistant.Algorithms.Strategies
{
    internal interface IStrategy<T> where T : ITypeDefinition
    {
        IEnumerable<IAssembly> DoAnalysis(T inputType);
    }
}
