using Lardite.RefAssistant.VsProxy.Building;

namespace Lardite.RefAssistant.VsProxy
{
    internal interface IVsProjectFacade
    {
        BuildResult Build(string projectName);
    }
}
