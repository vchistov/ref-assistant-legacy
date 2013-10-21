using System.Collections.Generic;
using Lardite.RefAssistant.Model.Projects;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    internal interface IVsProjectExtended : IVsProject
    {
        void RemoveReferences(IEnumerable<VsProjectReference> references);

        void RemoveAndSortUsings();
    }
}
