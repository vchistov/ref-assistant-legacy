using Lardite.RefAssistant.Algorithms.Data;

namespace Lardite.RefAssistant.Algorithms
{
    internal sealed class AssemblyManifestAlgorithm : IAlgorithm<IProject>
    {
        public AlgorithmResult Process(IProject project)
        {
            AlgorithmResult result = new AlgorithmResult();

            //add all manifest assemblies
            result.RequiredFor.AddRange(project.Assembly.References);

            return result;
        }
    }
}