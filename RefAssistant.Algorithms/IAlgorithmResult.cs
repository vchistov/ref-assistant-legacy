using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms
{
    internal interface IAlgorithmResult
    {
        string AlgorithmAdvice { get; }

        IProject Project { get; }

        IEnumerable<IProjectReference> RequiredFor { get; }
    }
}
