using System.Collections.Generic;
using Lardite.RefAssistant.Model.Projects;
using Microsoft.VisualStudio.FSharp.ProjectSystem.Automation;
using VSLangProj;

namespace Lardite.RefAssistant.VsProxy.Projects.References
{
    internal sealed class OAAssemblyReferenceController : VsReferenceController
    {
        public OAAssemblyReferenceController(VSProject project)
            : base(project)
        {}

        public override IEnumerable<VsProjectReference> References
        {
            get 
            {
                foreach (OAAssemblyReference projectRef in Project.References)
                {
                    yield return BuildReference(
                        projectRef.Name,
                        projectRef.Path,
                        projectRef.Version,
                        projectRef.Culture,
                        projectRef.PublicKeyToken,
                        false);
                }
            }
        }
    }
}
