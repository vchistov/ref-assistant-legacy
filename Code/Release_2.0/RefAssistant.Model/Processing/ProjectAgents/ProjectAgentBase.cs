using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.ReflectionServices;

namespace Lardite.RefAssistant.Model.Processing.ProjectAgents
{
    internal abstract class ProjectAgentBase
    {
        protected readonly IProject _project;
        protected readonly IServiceConfigurator _serviceConfigurator;

        protected ProjectAgentBase(IProject project, IServiceConfigurator serviceConfigurator)
        {
            Contract.Requires(project != null);
            Contract.Requires(serviceConfigurator != null);

            _project = project;
            _serviceConfigurator = serviceConfigurator;
        }

        public IEnumerable<IProjectReference> DoAnalysis()
        {
            var candidates = _project.ProjectRefs.ToList();

            foreach (var algorithm in this.Algorithms)
            {
                if (candidates.Count == 0)
                    break;

                var algorithmResult = algorithm.Process(_project);

                var requiredReferences = candidates
                    .Join(
                        algorithmResult.RequiredFor,
                        outer => outer.Assembly,
                        inner => inner,
                        (outer, inner) => outer)
                    .ToList();

                foreach(var reference in requiredReferences)
                {
                    candidates.Remove(reference);
                }
            }

            return candidates;
        }

        protected abstract IEnumerable<IAlgorithmLauncher> Algorithms { get; }
    }
}
