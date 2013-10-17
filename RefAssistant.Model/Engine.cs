using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lardite.RefAssistant.Model.Projects;

namespace Lardite.RefAssistant.Model
{
    public sealed class Engine
    {
        public Task<IEnumerable<VsProjectReference>> FindUnusedReferences(IVsProject project)
        {
            return new Task<IEnumerable<VsProjectReference>>(
                () => Enumerable.Empty<VsProjectReference>());
        }
    }
}
