using Lardite.RefAssistant.Algorithms.Contracts;

namespace Lardite.RefAssistant.Model.Processing.Data.Loaders
{
    internal interface IAlgorithmInputLoader<TInput>
    {
        TInput Load(IProject project);
    }
}
