using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms
{
    internal interface IAlgorithm<TInput>
    {
        IAlgorithmResult Process(TInput input);
    }
}
