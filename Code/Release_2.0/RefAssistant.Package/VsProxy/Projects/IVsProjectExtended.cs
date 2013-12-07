using System.Collections.Generic;
using Lardite.RefAssistant.Model.Contracts;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    internal interface IVsProjectExtended : IVsProject
    {
        string Configuration { get; }

        void RemoveReferences(IEnumerable<VsProjectReference> references);

        void RemoveAndSortUsings();
    }
}
