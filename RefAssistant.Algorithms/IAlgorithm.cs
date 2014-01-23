using Lardite.RefAssistant.Algorithms.Contracts;

namespace Lardite.RefAssistant.Algorithms
{
    public interface IAlgorithm<TInput>
    {
        AlgorithmResult Process(TInput input);
    }
}
