using System.Collections.Generic;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.ReflectionServices;

namespace Lardite.RefAssistant.Model.Processing.ProjectAgents
{
    [ProjectKind(VsProjectKinds.VisualCpp)]
    internal sealed class VisualCppProjectAgent : ProjectAgentBase
    {
        public VisualCppProjectAgent(IProject project, IServiceConfigurator serviceConfigurator)
            : base(project, serviceConfigurator)
        {
        }

        protected override IEnumerable<IAlgorithmLauncher> Algorithms
        {
            get 
            {
                yield break;
            }
        }
    }
}
