using System.Collections.Generic;

using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.Collections;

namespace Lardite.RefAssistant.Algorithms
{
    public sealed class AlgorithmResult
    {
        public AlgorithmResult(ISet<IAssembly> requiredFor, string advice = null)
        {
            ThrowUtils.ArgumentNull(() => requiredFor);

            this.RequiredFor = requiredFor.AsReadOnly();
            this.AlgorithmAdvice = advice;
        }

        public string AlgorithmAdvice { get; private set; }

        public  IReadOnlyHashSet<IAssembly> RequiredFor { get; private set; }
    }
}