//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Lardite.RefAssistant.ObjectModel;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Extension manager.
    /// </summary>
    public sealed class ExtensionManager : IDisposable
    {
        #region Fields

        private IShellGateway _shellGateway;
        private EventHandlerList _eventList;

        #region Nested classes

        // Define a unique key for each event
        private static class Events
        {
            public static object ProgressChanged = new object();
        }

        #endregion

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shellGateway">Shell gateway.</param>
        public ExtensionManager(IShellGateway shellGateway)
        {
            if (shellGateway == null)
                throw Error.ArgumentNull("shellGateway");

            _shellGateway = shellGateway;
            _eventList = new EventHandlerList();
        }

        #endregion // .ctor

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
        /// Starts cleanup.
        /// </summary>
        public void StartCleanup()
        {
            if (_shellGateway == null && _eventList == null)
                throw Error.ObjectDisposed(this);

            var projectInfo = BuildActiveProjectAndGetInfo();
            if (projectInfo == null)
            {
                return;
            }

            LogManager.OutputLog.Information(Environment.NewLine + string.Format(Resources.RefAssistantPackage_StartProcess, projectInfo.Name,
                projectInfo.ConfigurationName, projectInfo.PlatformName));
            OnProgressChanged(new ProgressEventArgs(30, Resources.ExtensionManager_GettingUnusedReferences));

            IEnumerable<ProjectReference> unusedProjectReferences = GetUnusedReferences(projectInfo);
            if (!HasUnusedReferences(unusedProjectReferences))
                return;

            if (_shellGateway.CanShowUnusedReferencesWindow())
            {
                if (!ShowUnusedReferencesWindow(ref unusedProjectReferences))
                    return;

                if (!HasUnusedReferences(unusedProjectReferences))
                    return;
            }

            OnProgressChanged(new ProgressEventArgs(60, Resources.ExtensionManager_RemovingUnusedReferences));

            int removedReferencesAmount = _shellGateway.RemoveUnusedReferences(projectInfo, unusedProjectReferences);

            if (_shellGateway.CanRemoveUnusedUsings(projectInfo))
            {
                OnProgressChanged(new ProgressEventArgs(90, Resources.ExtensionManager_RemovingUnusedUsings));
                _shellGateway.RemoveUnusedUsings(projectInfo);
            }

            LogManager.OutputLog.Information(string.Format(Resources.RefAssistantPackage_EndProcess, removedReferencesAmount));
            OnProgressChanged(new ProgressEventArgs(100, Resources.ExtensionManager_RemoveReady));
        }

        #endregion // Public methods

        #region Private methods

        /// <summary>
        /// Gets unused references.
        /// </summary>
        /// <param name="projectInfo">Project info.</param>
        /// <returns>Unused references.</returns>
        private IEnumerable<ProjectReference> GetUnusedReferences(ProjectInfo projectInfo)
        {
            using (ReferenceInspector referenceResolver = new ReferenceInspector())
            {
                return referenceResolver.Inspect(new ProjectEvaluator(projectInfo));
            }
        }

        /// <summary>
        /// Has unused references.
        /// </summary>
        /// <param name="unusedProjectReferences">Unused project references.</param>
        /// <returns>If true, then has.</returns>
        private bool HasUnusedReferences(IEnumerable<ProjectReference> unusedProjectReferences)
        {
            if (unusedProjectReferences == null || unusedProjectReferences.Count() == 0)
            {
                LogManager.OutputLog.Information(Resources.RefAssistantPackage_EndProcessWithoutReferences);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Shows unused references window.
        /// </summary>
        /// <param name="unusedProjectReferences">Unused project references.</param>
        /// <returns>If true, then continue.</returns>
        private bool ShowUnusedReferencesWindow(ref IEnumerable<ProjectReference> unusedProjectReferences)
        {
            if (!_shellGateway.ShowUnusedReferencesWindow(ref unusedProjectReferences))
            {
                LogManager.OutputLog.Information(Resources.RefAssistantPackage_UserCancelledOperation);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get an active solution's  project.
        /// </summary>
        /// <returns>Returns active project information.</returns>
        private ProjectInfo BuildActiveProjectAndGetInfo()
        {
            try
            {
                if (_shellGateway.BuildProject(null))
                {
                    return _shellGateway.GetActiveProjectInfo();
                }
            }
            catch (Exception ex)
            {
                LogManager.ActivityLog.Error(Resources.ExtensionManager_CannotGetActiveProject, ex);
            }
            return null;
        }

        #endregion // Private methods
    }
}
