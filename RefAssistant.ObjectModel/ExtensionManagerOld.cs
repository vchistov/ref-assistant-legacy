//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using System.ComponentModel;
using System.Linq;

using Lardite.RefAssistant.ObjectModel;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Extension manager.
    /// </summary>
    [Obsolete("Use Lardite.RefAssistant.ExtensionManager instead of.")]
    public sealed class ExtensionManagerOld : IDisposable
    {
        private IShellGateway _shellGateway;
        private ILogManager _logManager;
        private EventHandlerList _eventList;

        #region Nested classes

        // Define a unique key for each event
        private static class Events
        {
            public static object ProgressChanged = new object();
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shellGateway">Shell gateway.</param>
        public ExtensionManagerOld(IShellGateway shellGateway, ILogManager logManager)
        {
            if (shellGateway == null)
                throw Error.ArgumentNull("shellGateway");
            if (logManager == null)
                throw Error.ArgumentNull("logManager");

            _shellGateway = shellGateway;
            _logManager = logManager;
            _eventList = new EventHandlerList();
        }

        #region IDisposable implementation

        /// <summary>
        /// Dispose the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_eventList != null)
                {
                    _eventList.Dispose();
                    _eventList = null;
                }

                if (_shellGateway != null)
                {
                    _shellGateway = null;
                }
            }
        }

        #endregion // IDisposable implementation

        #region Events

        #region OnProgressChanged

        /// <summary>
        /// Progress changed event.
        /// </summary>
        public event EventHandler<ProgressEventArgs> ProgressChanged
        {
            add
            {
                _eventList.AddHandler(Events.ProgressChanged, value);
            }
            remove
            {
                _eventList.RemoveHandler(Events.ProgressChanged, value);
            }
        }

        /// <summary>
        /// Occurs when the removing progress changed.
        /// </summary>
        private void OnProgressChanged(ProgressEventArgs args)
        {
            EventHandler<ProgressEventArgs> progressChanged = (EventHandler<ProgressEventArgs>)_eventList[Events.ProgressChanged];

            if (progressChanged != null)
            {
                progressChanged(this, args);
            }
        }

        #endregion

        #endregion

        #region Public methods

        /// <summary>
        /// Starts cleaning up of a project.
        /// </summary>
        public void StartProjectCleanup()
        {
            if (_shellGateway == null && _eventList == null)
                throw Error.ObjectDisposed(this);

            var projectInfo = BuildActiveProjectAndGetProjectInfo();
            if (projectInfo == null)
            {
                return;
            }

            _logManager.Information(Environment.NewLine
                + string.Format(Resources.ExtensionManager_StartProcess, projectInfo.Name, projectInfo.ConfigurationName, projectInfo.PlatformName));

            OnProgressChanged(new ProgressEventArgs(30, Resources.ExtensionManager_GettingUnusedReferences));

            var unusedProjectReferences = GetUnusedReferences(projectInfo);
            if (!HasUnusedReferences(unusedProjectReferences))
                return;

            // show confirmation window
            if (_shellGateway.IsRemovingConfirmationRequired)
            {
                if (!ConfirmRemoveUnusedReferences(unusedProjectReferences))
                    return;

                if (!HasUnusedReferences(unusedProjectReferences))
                    return;
            }

            OnProgressChanged(new ProgressEventArgs(60, Resources.ExtensionManager_RemovingUnusedReferences));

            int removedReferencesAmount = unusedProjectReferences.Result.UnusedReferences.Count();
            _shellGateway.RemoveUnusedReferences(unusedProjectReferences);

            if (_shellGateway.CanRemoveUnusedUsings(projectInfo.Name))
            {
                OnProgressChanged(new ProgressEventArgs(90, Resources.ExtensionManager_RemovingUnusedUsings));
                _shellGateway.RemoveUnusedUsings(projectInfo);
            }

            _logManager.Information(string.Format(Resources.ExtensionManager_EndProcess, removedReferencesAmount));
            OnProgressChanged(new ProgressEventArgs(100, Resources.ExtensionManager_RemoveReady));
        }

        #endregion // Public methods

        #region Private methods

        /// <summary>
        /// Gets unused references.
        /// </summary>
        /// <param name="projectInfo">Project info.</param>
        /// <returns>Unused references.</returns>
        private IInspectResult GetUnusedReferences(ProjectInfo projectInfo)
        {
            using (ReferenceInspector referenceResolver = new ReferenceInspector())
            {
                return new InspectResult(referenceResolver.Inspect(new ProjectEvaluator(projectInfo)));
            }
        }

        /// <summary>
        /// Has unused references.
        /// </summary>
        /// <param name="inspectResults">Unused project references.</param>
        /// <returns>If true, then has.</returns>
        private bool HasUnusedReferences(IInspectResult inspectResults)
        {
            if (inspectResults == null || !inspectResults.HasUnusedReferences)
            {
                _logManager.Information(Resources.ExtensionManager_EndProcessWithoutReferences);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Shows unused references window.
        /// </summary>
        /// <param name="inspectResults">Unused project references.</param>
        /// <returns>If true, then continue.</returns>
        private bool ConfirmRemoveUnusedReferences(IInspectResult inspectResults)
        {
            if (!_shellGateway.ConfirmUnusedReferencesRemoving(inspectResults))
            {
                _logManager.Information(Resources.ExtensionManager_UserCancelledOperation);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get an active solution's  project.
        /// </summary>
        /// <returns>Returns active project information.</returns>
        private ProjectInfo BuildActiveProjectAndGetProjectInfo()
        {
            try
            {
                var result = _shellGateway.BuildProject(null);
                if (!result.IsClrAssembly && result.IsSuccessed)
                {
                    _logManager.Warning(Resources.ExtensionManager_IsNotClrAssembly);

                    return null;
                }

                if (result.IsSuccessed)
                {
                    return _shellGateway.GetActiveProjectInfo();
                }
            }
            catch (Exception ex)
            {
                _logManager.Error(Resources.ExtensionManager_CannotGetActiveProject, ex);
            }

            return null;
        }        

        #endregion // Private methods
    }
}