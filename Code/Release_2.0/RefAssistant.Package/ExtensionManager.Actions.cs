using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

using Lardite.RefAssistant.Model;
using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.UI;
using Lardite.RefAssistant.VsProxy;
using Lardite.RefAssistant.VsProxy.Building;
using Lardite.RefAssistant.VsProxy.Projects;

namespace Lardite.RefAssistant
{
    internal partial class ExtensionManager
    {
        private IVsProjectExtended BuildProject(IVsProject project)
        {
            Contract.Requires(project != null);

            BuildResult result = _vsFacade.Build(project);

            if (!result.IsSuccessed)
                throw new ActionInterruptedException(Resources.ExtensionManager_BuildError);

            return result.Project;
        }

        private IEnumerable<VsProjectReference> GetUnusedReferences(IVsProject project)
        {
            Contract.Requires(project != null);

            try
            {
                var engine = new Engine();
                var task = engine.FindUnusedReferences(project);

                var referenses = task.Result;
                
                if (!referenses.Any())
                    throw new ActionInterruptedException(Resources.ExtensionManager_NotFound);
             
                return referenses;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is BadImageFormatException)
                    throw new ActionInterruptedException(Resources.ExtensionManager_IsNotClrAssembly, ex);

                throw new InvalidOperationException(
                    Resources.ExtensionManager_AnalysisError,
                    (ex.InnerExceptions.Count == 1 && ex.InnerException != null) ? ex.InnerException : ex);
            }
        }

        private void RemoveProjectReferences(IVsProjectExtended project, IEnumerable<VsProjectReference> references)
        {
            Contract.Requires(project != null);
            Contract.Requires(references != null);
            Contract.Requires(references.Any());

            IEnumerable<VsProjectReference> readyForRemoveRefs = references;
            if (_options.IsShowUnusedReferencesWindow.GetValueOrDefault())
            {
                readyForRemoveRefs = ConfirmUnusedReferencesRemoving(references);

                if (!readyForRemoveRefs.Any())
                    throw new ActionInterruptedException(Resources.ExtensionManager_NotFound);
            }

            project.RemoveReferences(readyForRemoveRefs);
            PrintUnusedReferences(readyForRemoveRefs);
        }

        private void RemoveAndSortUsings(IVsProjectExtended project)
        {
            Contract.Requires(project != null);

            if (_options.IsRemoveUsingsAfterRemoving.GetValueOrDefault())
            {
                project.RemoveAndSortUsings();
            }
        }

        #region Helpers

        public IEnumerable<VsProjectReference> ConfirmUnusedReferencesRemoving(IEnumerable<VsProjectReference> references)
        {
            var window = new UnusedReferencesWindow(references)
                {
                    IsShowThisWindowAgain = _options.IsShowUnusedReferencesWindow.Value
                };

            var result = window.ShowModal();
            if (result.GetValueOrDefault())
            {
                _options.IsShowUnusedReferencesWindow = window.IsShowThisWindowAgain;
                return window.ViewModel.SelectedReferences.ToList();
            }

            throw new ActionInterruptedException(Resources.ExtensionManager_UserCancelledOperation);
        }

        private void PrintUnusedReferences(IEnumerable<VsProjectReference> references)
        { 
            StringBuilder builder = new StringBuilder();
            
            foreach (var @ref in references)
            {
                builder.Append("  ").AppendLine(@ref.Name /* TODO: FullName */);
            }

            LogManager.OutputLog.Information(builder.ToString().TrimEnd());
        }

        #endregion
    }
}
