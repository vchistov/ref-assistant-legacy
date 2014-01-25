using System.Diagnostics.Contracts;
using Lardite.RefAssistant.Algorithms.Contracts;

namespace Lardite.RefAssistant.Model.Processing.Data.Loaders
{
    internal sealed class ProjectAssemblyInputLoader : IAlgorithmInputLoader<IAssembly>
    {
        public IAssembly Load(IProject project)
        {
            Contract.Requires(project != null);
            Contract.Ensures(Contract.Result<IAssembly>() != null);

            return project.Assembly;
        }
    }
}
