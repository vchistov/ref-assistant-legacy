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
                IVsProjectExtended compiledProject = BuildProject(project);
                IEnumerable<VsProjectReference> references = GetUnusedReferences(compiledProject);
                RemoveProjectReferences(compiledProject, references);
                RemoveAndSortUsings(compiledProject);
            }
            catch (ActionInterruptedException ex)
            {
                // TODO: update logging
                LogManager.OutputLog.Information(ex.Message);
            }
        }

        #region Nested types

        [Serializable]
        private sealed class ActionInterruptedException : ApplicationException
        {
            public ActionInterruptedException(string message = null, Exception innerException = null)
                : base(message, innerException)
            {
            }
        }

        #endregion
    }
}
