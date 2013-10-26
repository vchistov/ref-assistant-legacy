using System.Collections.Generic;
using Lardite.RefAssistant.Model.Projects;
using VSLangProj;
using VSLangProj80;

namespace Lardite.RefAssistant.VsProxy.Projects.References
{
    internal sealed class Reference3Controller : VsReferenceController
    {
        public Reference3Controller(VSProject project)
            : base(project)
        {}

        public override IEnumerable<VsProjectReference> References
        {
            get 
            {
                foreach (Reference3 projectRef in Project.References)
                {
                    yield return BuildReference(
                        projectRef.Name,
                        projectRef.Path,
                        projectRef.Version,
                        projectRef.Culture,
                        projectRef.PublicKeyToken,
                        projectRef.SpecificVersion);
                }
            }
        }
    }
}
