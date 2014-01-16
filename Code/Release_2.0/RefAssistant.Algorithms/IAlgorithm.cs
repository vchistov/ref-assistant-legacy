using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms
{
    public interface IAlgorithm<TInput>
    {
        AlgorithmResult Process(TInput input);
    }
}
