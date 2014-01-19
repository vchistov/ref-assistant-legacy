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
        private BuildResult BuildProject(IVsProject project)
        {
            Contract.Requires(project != null);

            return _vsFacade.Build(project);
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
                    throw new ActionInterruptedException(Resources.ExtensionManager_Break_NotFound);
             
                return referenses;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is BadImageFormatException)
                    throw new ActionInterruptedException(Resources.ExtensionManager_Break_IsNotCliAssembly);

                throw new InvalidOperationException(
                    Resources.ExtensionManager_AnalysisError,
                    (ex.InnerExceptions.Count == 1 && ex.InnerException != null) ? ex.InnerException : ex);
            }
        }

        private int RemoveProjectReferences(IVsProjectExtended project, IEnumerable<VsProjectReference> references)
        {
            Contract.Requires(project != null);
            Contract.Requires(references != null);
            Contract.Requires(references.Any());

            IList<VsProjectReference> readyForRemoveRefs = references.ToList();
            if (_options.IsShowUnusedReferencesWindow.GetValueOrDefault())
            {
                readyForRemoveRefs = ConfirmUnusedReferencesRemoving(references).ToList();

                if (readyForRemoveRefs.Count == 0)
                    throw new ActionInterruptedException(Resources.ExtensionManager_Break_Cancelled);
            }

            project.RemoveReferences(readyForRemoveRefs);
            PrintUnusedReferences(readyForRemoveRefs);

            return readyForRemoveRefs.Count;
        }

        private void RemoveAndSortUsings(IVsProjectExtended project)
        {
            Contract.Requires(project != null);

            if (_options.IsRemoveUsingsAfterRemoving.GetValueOrDefault())
            {
                LogManager.Instance.Information(Environment.NewLine + "  " + Resources.ExtensionManager_RemovingUnusedUsings);
                project.RemoveAndSortUsings();
            }
        }

        #region Helpers

        private IEnumerable<VsProjectReference> ConfirmUnusedReferencesRemoving(IEnumerable<VsProjectReference> references)
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

            throw new ActionInterruptedException(Resources.ExtensionManager_Break_Cancelled);
        }

        private void PrintUnusedReferences(IEnumerable<VsProjectReference> references)
        { 
            StringBuilder builder = new StringBuilder();
            
            foreach (var @ref in references)
            {
                builder.Append("  ").AppendLine(@ref.Name /* TODO: FullName */);
            }

            LogManager.Instance.Information(builder.ToString().TrimEnd());
        }

        #endregion
    }
}