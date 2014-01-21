using System.Diagnostics.Contracts;
using Lardite.RefAssistant.Algorithms;
using Lardite.RefAssistant.Algorithms.Data;
using Lardite.RefAssistant.Model.Processing.Data.Loaders;

namespace Lardite.RefAssistant.Model.Processing
{
    internal class AlgorithmLauncher<TAlgorithm, TInput> : IAlgorithmLauncher
        where TAlgorithm : IAlgorithm<TInput>, new()
    {
        private readonly IAlgorithm<TInput> _algorithm;
        private readonly IAlgorithmInputLoader<TInput> _inputLoader;

        public AlgorithmLauncher(IAlgorithmInputLoader<TInput> inputLoader)
        {
            Contract.Requires(inputLoader != null);

            _algorithm = new TAlgorithm();
            _inputLoader = inputLoader;
        }

        AlgorithmResult IAlgorithmLauncher.Process(IProject project)
        {
            Contract.Requires(project != null);
            Contract.Ensures(Contract.Result<AlgorithmResult>() != null);

            var input = _inputLoader.Load(project);
            Contract.Assert(input != null);
            return _algorithm.Process(input);
        }
    }
}
