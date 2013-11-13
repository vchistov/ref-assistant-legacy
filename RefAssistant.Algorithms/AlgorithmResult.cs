using System.Collections.Generic;

using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms
{
    internal sealed class AlgorithmResult
    {
        public string AlgorithmAdvice
        {
            get;
            set;
        }

        public List<IAssembly> RequiredFor
        {
            get;
            set;
        }
    }
}