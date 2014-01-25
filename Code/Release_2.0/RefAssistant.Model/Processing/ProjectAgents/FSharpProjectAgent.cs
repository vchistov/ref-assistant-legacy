using System.Collections.Generic;
using Lardite.RefAssistant.Algorithms;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.Model.Processing.Data.Loaders;
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
                yield return new AlgorithmLauncher<TypeInheritanceAlgorithm, IEnumerable<ITypeDefinition>>(
                    new TypeDefinitionsInputLoader());
            }
        }   
    }
}
