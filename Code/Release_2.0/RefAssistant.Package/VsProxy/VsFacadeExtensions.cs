using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.VsProxy.Building;

namespace Lardite.RefAssistant.VsProxy
{
    internal static class VsFacadeExtensions
    {
        public static BuildResult Build(this IVsProjectFacade @this, IVsProject project)
        {
            ThrowUtils.ArgumentNull(() => @this);
            ThrowUtils.ArgumentNull(() => project);
            
            return @this.Build(project.Name);
        }
    }
}
