using Lardite.RefAssistant.VsProxy.Projects;

namespace Lardite.RefAssistant.VsProxy.Building
{
    internal sealed class BuildResult
    {
        public BuildResult(IVsProjectExtended project, bool isSuccessed = true)
        {
            Project = project;
            IsSuccessed = isSuccessed;
        }

        public IVsProjectExtended Project { get; private set; }

        public bool IsSuccessed { get; private set; }
    }
}
