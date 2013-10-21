using Lardite.RefAssistant.VsProxy.Projects;

namespace Lardite.RefAssistant.VsProxy
{
    internal interface IVsSolutionFacade
    {
        IVsProjectExtended GetActiveProject();

        IVsProjectExtended GetProject(string projectName);

        bool IsBuildInProgress();
   } 
}
