using System.Collections.Generic;

using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms
{
    public sealed class AssemblyManifestAlgorithm : IAlgorithm<IProject>
    {
        AlgorithmResult IAlgorithm<IProject>.Process(IProject project)
        {
            return new AlgorithmResult(
                new HashSet<IAssembly>(project.Assembly.References),
                this.GetType().FullName);
        }
    }
}