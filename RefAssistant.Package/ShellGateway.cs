//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lardite.RefAssistant.ObjectModel;
using Lardite.RefAssistant.UI;
using Lardite.RefAssistant.VsProxy;
using Lardite.RefAssistant.VsProxy.Projects;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Shell gateway.
    /// </summary>
    public sealed class ShellGateway : IShellGateway
    {
        #region Fields

        private readonly IServiceProvider _serviceProvider;
        private readonly IExtensionOptions _options;
        private readonly ProjectsCache _projectsCache;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="options">Extension options.</param>
        public ShellGateway(IServiceProvider serviceProvider, IExtensionOptions options)
        {
            _serviceProvider = serviceProvider;
            _options = options;
            _projectsCache = new ProjectsCache(_serviceProvider);
        }

        #endregion // .ctor

        #region Public methods

        /// <summary>
        /// Builds project.
        /// </summary>
        /// <param name="projectInfo">The project information. If null then builds active project.</param>
        /// <returns>Returns true if success; otherwise false.</returns>
        public CompilationInfo BuildProject(ProjectInfo projectInfo)
        {
            var project = GetProjectWrapper(projectInfo);
            var buildResult = project.Build();
            if (buildResult != 0)
            {
                DTEHelper.ShowErrorList(_serviceProvider);
            }
            return new CompilationInfo(buildResult == 0, project.HasAssembly);
        }

        /// <summary>
        /// Builds current solution.
        /// </summary>        
        /// <returns>Returns true if success; otherwise false.</returns>
        public bool BuildSolution()
        {
            var buildResult = DTEHelper.BuildSolution(_serviceProvider);
            if (buildResult != 0)
            {
                DTEHelper.ShowErrorList(_serviceProvider);
            }
            return buildResult == 0;
        }

        /// <summary>
        /// Gets active project info.
        /// </summary>
        /// <returns>Active project info.</returns>
        public ProjectInfo GetActiveProjectInfo()
        {
            return GetProjectWrapper(null).GetProjectInfo();
        }

        /// <summary>
        /// Gets project info by name.
        /// </summary>
        /// <param name="projectName">The project name.</param>
        /// <returns>The project info.</returns>
        public ProjectInfo GetProjectInfo(string projectName)
        {
            return GetProjectWrapperByName(projectName).GetProjectInfo();
        }

        /// <summary>
        /// Gets solution's projects.
        /// </summary>
        /// <param name="unsupportedProjects">The list of unsupported projects.</param>
        /// <returns>The projects list.</returns>
        public IEnumerable<ProjectInfo> GetSolutionProjects(out IEnumerable<string> unsupportedProjects)
        {
            var unsupported = new List<string>();
            unsupportedProjects = unsupported;
            var projects = DTEHelper.GetSolutionProjects(_serviceProvider);
            var projectsInfo = new List<ProjectInfo>();

            foreach (var project in projects)
            {
                var projectName = project.Name;
                if (CanRemoveUnusedReferences(projectName))
                {
                    projectsInfo.Add(GetProjectWrapperByName(projectName).GetProjectInfo());
                }
                else
                {
                    unsupported.Add(projectName);
                }
            }

            return projectsInfo;
        }

        /// <summary>
        /// Can show unused references window.
        /// </summary>
        /// <returns>If true, then can.</returns>
        public bool IsRemovingConfirmationRequired
        {
            get
            {
                return _options.IsShowUnusedReferencesWindow.HasValue
                    && _options.IsShowUnusedReferencesWindow.Value;
            }
        }

        /// <summary>
        /// Shows unused references window.
        /// </summary>
        /// <param name="unusedProjectReferences">Unused project references.</param>
        /// <returns>If true, then continue.</returns>
        public bool ConfirmUnusedReferencesRemoving(IInspectResult inspectResults)
        {
            var window = new UnusedReferencesWindow(inspectResults)
                {
                    IsShowThisWindowAgain = _options.IsShowUnusedReferencesWindow.Value
                };
            var result = window.ShowModal();
            if (result.HasValue && result.Value)
            {
                //unusedProjectReferences = window.GetUnusedReferences();
                _options.IsShowUnusedReferencesWindow = window.IsShowThisWindowAgain;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Can remove unused references.
        /// </summary>
        /// <param name="projectName">The project name. If null then gets active project.</param>
        public bool CanRemoveUnusedReferences(string projectName)
        {
            try
            {
                var project = GetProjectWrapperByName(projectName);
                var vcpp = project as VisualCppCliProjectWrapper;
                if (vcpp != null && !vcpp.IsManaged)
                {
                    return false;
                }

                return (project.HasAssembly 
                    && project.Kind != ProjectKinds.Modeling
                    && project.Kind != ProjectKinds.Database
                    && !project.IsBuildInProgress);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes unused references.
        /// </summary>
        /// <param name="inspectResults">Unused project references.</param>
        public void RemoveUnusedReferences(IInspectResult inspectResults)
        {
            foreach (var projectInspectResult in inspectResults.InspectResults.Where(p => p.IsSuccess))
            {
                RemoveUnusedReferences(projectInspectResult.Project, projectInspectResult.UnusedReferences);
            }
        }

        /// <summary>
        /// Can remove unused using. Checks only settings.
        /// </summary>        
        /// <returns>If true, then can.</returns>
        public bool CanRemoveUnusedUsings()
        {
            try
            {
                return _options.IsRemoveUsingsAfterRemoving.HasValue
                    && _options.IsRemoveUsingsAfterRemoving.Value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Can remove unused references.
        /// </summary>
        /// <param name="projectName">The project name. If null then gets active project.</param>
        /// <returns>If true, then can.</returns>
        public bool CanRemoveUnusedUsings(string projectName)
        {
            try
            {
                var project = GetProjectWrapperByName(projectName);
                return (CanRemoveUnusedUsings()
                    && project.Kind == ProjectKinds.CSharp
                    && !project.IsBuildInProgress);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes unused usings.
        /// </summary>
        /// <param name="projectInfo">The project information. If null then gets active project.</param>
        public void RemoveUnusedUsings(ProjectInfo projectInfo)
        {
            var project = GetProjectWrapper(projectInfo);
            LogManager.OutputLog.Information(
                string.Format("  {0} -> {1}", projectInfo.Name, Resources.RefAssistantPackage_RemoveUnusedUsings));
            project.RemoveUnusedUsings(_serviceProvider);
        }

        #endregion // Public methods

        #region Private methods

        /// <summary>
        /// Gets cached project wrapper.
        /// </summary>
        /// <param name="projectInfo">The project information. If null then gets active project.</param>
        /// <returns>Returns the project wrapper.</returns>
        /// <exception cref="System.InvalidOperationException"/>        
        private BaseProjectWrapper GetProjectWrapper(ProjectInfo projectInfo)
        {
            return GetProjectWrapperByName(projectInfo != null ? projectInfo.Name : null);
        }

        /// <summary>
        /// Gets cached project wrapper.
        /// </summary>
        /// <param name="projectName">The project name. If null or empty then gets active project.</param>
        /// <returns>Returns the project wrapper.</returns>
        /// <exception cref="System.InvalidOperationException"/>        
        private BaseProjectWrapper GetProjectWrapperByName(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                // gets active project
                var project = DTEHelper.GetActiveProject(_serviceProvider);
                if (project == null)
                {
                    throw Error.InvalidOperation(Resources.ShellGateway_CannotGetActiveProject);
                }

                return CreateProjectWrapperOrGetCached(project.Name);
            }

            // gets project by name
            return CreateProjectWrapperOrGetCached(projectName);
        }

        /// <summary>
        /// Gets cached project wrapper by project name.
        /// </summary>
        /// <param name="projectName">The project name.</param>
        /// <returns>Returns the project wrapper.</returns>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.InvalidOperationException"/> 
        private BaseProjectWrapper CreateProjectWrapperOrGetCached(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                throw Error.ArgumentNull("projectName", Resources.ShellGateway_ProjectNameIsNull);
            }

            var wrapper = _projectsCache[projectName];
            if (wrapper == null)
            {
                throw Error.InvalidOperation(string.Format(Resources.ShellGateway_ProjectNotFound, projectName));
            }

            return wrapper;
        }

        /// <summary>
        /// Removes unused references.
        /// </summary>
        /// <param name="projectInfo">The project information. If null then gets active project.</param>
        /// <param name="unusedProjectReferences">Unused project references.</param>
        /// <returns>Removed references count.</returns>
        private void RemoveUnusedReferences(ProjectInfo projectInfo, IEnumerable<ProjectReference> unusedProjectReferences)
        {
            var project = GetProjectWrapper(projectInfo);

            StringBuilder builder = new StringBuilder();
            IEnumerable<ProjectReference> unusedReferences = project.RemoveUnusedReferences(unusedProjectReferences);

            foreach (var unusedReference in unusedReferences)
            {
                builder.Append("  ")
                    .Append(projectInfo.Name)
                    .Append(" -> ")
                    .AppendLine(unusedReference.FullName);
            }

            LogManager.OutputLog.Information(builder.ToString().TrimEnd());
        }

        #endregion // Private methods

        #region Nested class

        /// <summary>
        /// Cache for solution's projects.
        /// </summary>
        private class ProjectsCache : SimpleCache<string, BaseProjectWrapper>
        {
            #region .ctor

            private readonly IServiceProvider _serviceProvider;
            public ProjectsCache(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            #endregion // .ctor

            #region Overriding

            protected override BaseProjectWrapper GetValue(string key)
            {
                return DTEHelper.CreateProjectWrapper(DTEHelper.GetProjectByName(_serviceProvider, key));
            }

            #endregion // Overriding
        }

        #endregion // Nested class
    }
}
