using System.Collections.Generic;
using Lardite.RefAssistant.Algorithms.Data;
using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.ReflectionServices;

namespace Lardite.RefAssistant.Model.Processing.ProjectAgents
{
    [ProjectKind(VsProjectKinds.FSharp)]
    internal sealed class FSharpProjectAgent : ProjectAgentBase
    {
        public FSharpProjectAgent(IProject project, IServiceConfigurator serviceConfigurator)
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
