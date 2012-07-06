//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            LogManager.OutputLog.Information(Environment.NewLine + string.Format(Resources.ExtensionManager_StartProcess, projectInfo.Name,
                projectInfo.ConfigurationName, projectInfo.PlatformName));
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

            int removedReferencesAmount = unusedProjectReferences.InspectResults.First().UnusedReferences.Count();
            _shellGateway.RemoveUnusedReferences(unusedProjectReferences);

            if (_shellGateway.CanRemoveUnusedUsings(projectInfo.Name))
            {
                OnProgressChanged(new ProgressEventArgs(90, Resources.ExtensionManager_RemovingUnusedUsings));
                _shellGateway.RemoveUnusedUsings(projectInfo);
            }

            LogManager.OutputLog.Information(string.Format(Resources.ExtensionManager_EndProcess, removedReferencesAmount));
            OnProgressChanged(new ProgressEventArgs(100, Resources.ExtensionManager_RemoveReady));
        }

        /// <summary>
        /// Starts cleaning up of solution's projects.
        /// </summary>
        public void StartSolutionCleanup()
        {
            if (_shellGateway == null && _eventList == null)
                throw Error.ObjectDisposed(this);

            IEnumerable<string> unsupportedProjects;
            var projectsInfo = BuildSolutionAndGetProjectsNames(out unsupportedProjects);

            LogManager.OutputLog.Information(Environment.NewLine + Resources.ExtensionManager_StartSolutionCleanup);

            ShowUnsupportedProjectsList(unsupportedProjects);

            if (!HasProjects(projectsInfo))
            {
                return;
            }

            OnProgressChanged(new ProgressEventArgs(30, Resources.ExtensionManager_GettingUnusedReferences));

            var unusedProjectReferences = GetUnusedReferences(projectsInfo);
            if (!HasUnusedReferences(unusedProjectReferences))
            {
                return;
            }

            if (_shellGateway.IsRemovingConfirmationRequired)
            {
                if (!ConfirmRemoveUnusedReferences(unusedProjectReferences))
                    return;

                if (!HasUnusedReferences(unusedProjectReferences))
                    return;
            }

            OnProgressChanged(new ProgressEventArgs(60, Resources.ExtensionManager_RemovingUnusedReferences));

            int removedReferencesAmount = unusedProjectReferences.InspectResults.Sum(p => p.UnusedReferences.Count());
            RemoveUnusedReferences(unusedProjectReferences);
            RemoveUnusedUsings(unusedProjectReferences);

            LogManager.OutputLog.Information(Environment.NewLine + string.Format(Resources.ExtensionManager_EndProcess, removedReferencesAmount));
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
                return new InspectResult(new[] { referenceResolver.Inspect(new ProjectEvaluator(projectInfo)) });
            }
        }

        /// <summary>
        /// Get unused references of projects list.
        /// </summary>
        /// <param name="projectsInfo">The list of projects.</param>
        /// <returns>Returns unused references for porjects.</returns>
        private IInspectResult GetUnusedReferences(IEnumerable<ProjectInfo> projectsInfo)
        {
            var sharedData = new ConcurrentBag<IProjectInspectResult>();

            var loopOptions = new ParallelOptions();
            loopOptions.MaxDegreeOfParallelism = Environment.ProcessorCount;

            using (ReferenceInspector referenceResolver = new ReferenceInspector())
            {
                Parallel.ForEach<ProjectInfo>(projectsInfo, loopOptions, (projectInfo) =>
                {
                    try
                    {
                        var result = referenceResolver.Inspect(new ProjectEvaluator(projectInfo));
                        sharedData.Add(result);
                    }
                    catch (Exception ex)
                    {
                        sharedData.Add(new ProjectInspectResult(projectInfo, ex));
                    }
                });
            }

            return new InspectResult(sharedData);
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
                LogManager.OutputLog.Information(Resources.ExtensionManager_EndProcessWithoutReferences);
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
                LogManager.OutputLog.Information(Resources.ExtensionManager_UserCancelledOperation);
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
                    LogManager.ActivityLog.Warning(Resources.ExtensionManager_IsNotClrAssembly);
                    LogManager.OutputLog.Warning(Resources.ExtensionManager_IsNotClrAssembly);
                    LogManager.ErrorListLog.Warning(Resources.ExtensionManager_IsNotClrAssembly);

                    return null;
                }

                if (result.IsSuccessed)
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

        /// <summary>
        /// Get an active solution's  project.
        /// </summary>
        /// <returns>Returns active project information.</returns>
        private IEnumerable<ProjectInfo> BuildSolutionAndGetProjectsNames(out IEnumerable<string> unsupportedProjects)
        {
            try
            {
                if (_shellGateway.BuildSolution())
                {
                    return _shellGateway.GetSolutionProjects(out unsupportedProjects);
                }
            }
            catch (Exception ex)
            {
                LogManager.ActivityLog.Error(Resources.ExtensionManager_CannotGetProjectsInfo, ex);
            }
            unsupportedProjects = null;
            return null;
        }

        /// <summary>
        /// Shows the projects list which type is not supported by the extension.
        /// </summary>
        /// <param name="unsupportedProjects">The unsupported projects list.</param>
        private void ShowUnsupportedProjectsList(IEnumerable<string> unsupportedProjects)
        {
            if (unsupportedProjects != null)
            {
                var sb = new StringBuilder();
                foreach (var project in unsupportedProjects)
                {
                    sb.Append("  ")
                        .Append(project)
                        .Append(" -> ")
                        .AppendLine(Resources.ExtensionManager_UnsupportedProject);
                }
                LogManager.OutputLog.Warning(sb.ToString().TrimEnd());
            }
        }

        /// <summary>
        /// Checks the projects amount.
        /// </summary>
        /// <param name="projects">The projects for analysis.</param>
        /// <returns>Return true if projects more than 0; otherwise false.</returns>
        private bool HasProjects(IEnumerable<ProjectInfo> projects)
        {
            if (projects == null || projects.Count() < 1)
            {
                LogManager.OutputLog.Information(Resources.ExtensionManager_NoProjects);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Removes unused references from list of the projects.
        /// </summary>
        /// <param name="unusedProjectRefs">The list of the projects with unused references.</param>
        private void RemoveUnusedReferences(IInspectResult inspectResults)
        {
            var sb = new StringBuilder();
            foreach (var projectInspectResult in inspectResults.InspectResults)
            {
                if (projectInspectResult.UnusedReferences.Count() < 1)
                {
                    sb.Append("  ")
                        .Append(projectInspectResult.Project.Name)
                        .Append(" -> ")
                        .AppendLine(Resources.ExtensionManager_NoReferencesForProject);
                }
            }
            LogManager.OutputLog.Information(sb.ToString().TrimEnd());

            _shellGateway.RemoveUnusedReferences(inspectResults);
        }

        /// <summary>
        /// Removes unused usings from project's source code files.
        /// </summary>
        /// <param name="unusedProjectRefs">The list of the projects with unused references.</param>
        private void RemoveUnusedUsings(IInspectResult inspectResults)
        {
            if (!_shellGateway.CanRemoveUnusedUsings())
            {
                return;
            }

            OnProgressChanged(new ProgressEventArgs(90, Resources.ExtensionManager_RemovingUnusedUsings));

            foreach (var projectInspectResult in inspectResults.InspectResults)
            {
                if (_shellGateway.CanRemoveUnusedUsings(projectInspectResult.Project.Name))
                {
                    _shellGateway.RemoveUnusedUsings(projectInspectResult.Project);
                }
            }
        }

        ///// <summary>
        ///// Create the <see cref="IUnusedProjectReferences"/> instance for single project.
        ///// </summary>
        ///// <param name="projectInfo">The project.</param>
        ///// <param name="unusedProjectReferences">The unused references of the project.</param>
        ///// <returns>Returns the <see cref="IUnusedProjectReferences"/> instance.</returns>
        //private IUnusedProjectReferences CreateUnusedReferencesObject(ProjectInfo projectInfo, IEnumerable<ProjectReference> unusedProjectReferences)
        //{
        //    var upr = new UnusedProjectReferences();
        //    upr.Add(projectInfo, unusedProjectReferences);

        //    return upr;
        //}

        ///// <summary>
        ///// Create the <see cref="IUnusedProjectReferences"/> instance for several projects.
        ///// </summary>
        ///// <param name="unusedProjectRefs">The list of the projects with unused references.</param>
        ///// <returns>Returns the <see cref="IUnusedProjectReferences"/> instance.</returns>
        //private IUnusedProjectReferences CreateUnusedReferencesObject(IDictionary<ProjectInfo, IEnumerable<ProjectReference>> unusedProjectRefs)
        //{
        //    var upr = new UnusedProjectReferences();
        //    foreach (var project in unusedProjectRefs)
        //    {
        //        upr.Add(project.Key, project.Value);
        //    }
        //    return upr;
        //}

        #endregion // Private methods
    }
}
