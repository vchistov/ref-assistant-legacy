using Lardite.RefAssistant.Algorithms;
using Lardite.RefAssistant.Algorithms.Contracts;

namespace Lardite.RefAssistant.Model.Processing
{
    internal interface IAlgorithmLauncher
    {
        AlgorithmResult Process(IProject project);
    }
}
