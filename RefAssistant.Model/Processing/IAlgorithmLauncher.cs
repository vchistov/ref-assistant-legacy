using Lardite.RefAssistant.Algorithms;
using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Model.Processing
{
    internal interface IAlgorithmLauncher
    {
        AlgorithmResult Process(IProject project);
    }
}
