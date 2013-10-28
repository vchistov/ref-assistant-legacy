using System;
using System.Collections;
using System.Collections.Generic;

using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.VsProxy;
using Lardite.RefAssistant.VsProxy.Projects;

namespace Lardite.RefAssistant
{
    internal partial class ExtensionManager
    {
        private readonly IVsProjectFacade _vsFacade;
        private readonly IExtensionOptions _options;

        public ExtensionManager(IExtensionOptions options, IVsProjectFacade vsFacade)
        {
            ThrowUtils.ArgumentNull(() => options);
            ThrowUtils.ArgumentNull(() => vsFacade);

            _options = options;
            _vsFacade = vsFacade;
        }

        public void ProcessProject(IVsProjectExtended project)
        {
            ThrowUtils.ArgumentNull(() => project);

            try
            {
                var result = BuildProject(project);
                if (!result.IsSuccessed)
                {
                    LogManager.Instance.Error(Resources.ExtensionManager_BuildError);
                    return;
                }

                IVsProjectExtended compiledProject = result.Project;

                LogManager.Instance.Information(string.Format(Resources.ExtensionManager_StartProcess, compiledProject.Name, compiledProject.Configuration));

                IEnumerable<VsProjectReference> references = GetUnusedReferences(compiledProject);
                int count = RemoveProjectReferences(compiledProject, references);
                RemoveAndSortUsings(compiledProject);

                LogManager.Instance.Information(string.Format(Resources.ExtensionManager_EndProcess, count));
            }
            catch (ActionInterruptedException ex)
            {
                LogManager.Instance.Information(string.Format(Resources.ExtensionManager_Break_EndProcess, ex.Message));
            }
        }

        #region Nested types

        [Serializable]
        private sealed class ActionInterruptedException : ApplicationException
        {
            public ActionInterruptedException(string message)
                : base(message)
            {
            }
        }

        #endregion
    }
}
