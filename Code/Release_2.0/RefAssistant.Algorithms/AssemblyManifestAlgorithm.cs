using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Lardite.RefAssistant.Algorithms.Contracts;

namespace Lardite.RefAssistant.Algorithms
{
    public sealed class AssemblyManifestAlgorithm : IAlgorithm<IAssembly>
    {
        public AlgorithmResult Process(IAssembly assembly)
        {
            Contract.Requires(assembly != null);
            Contract.Ensures(Contract.Result<AlgorithmResult>() != null);

            return new AlgorithmResult(
                new HashSet<IAssembly>(assembly.References),
                this.GetType().FullName);
        }
    }
}