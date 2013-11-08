using System.Collections.Generic;
using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms
{
    internal interface IAlgorithmResult
    {
        string AlgorithmAdvice { get; }

        IEnumerable<IAssembly> RequiredFor { get; }
    }
}
