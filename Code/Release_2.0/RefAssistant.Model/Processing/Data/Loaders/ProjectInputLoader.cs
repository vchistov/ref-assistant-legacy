using System.Diagnostics.Contracts;
using Lardite.RefAssistant.Algorithms.Contracts;

namespace Lardite.RefAssistant.Model.Processing.Data.Loaders
{
    internal sealed class ProjectInputLoader : IAlgorithmInputLoader<IProject>
    {
        public IProject Load(IProject project)
        {
            Contract.Requires(project != null);
            Contract.Ensures(Contract.Result<IProject>() != null);

            return project;
        }
    }
}
