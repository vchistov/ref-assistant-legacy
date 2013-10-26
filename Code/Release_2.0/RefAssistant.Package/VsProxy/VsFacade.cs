using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using EnvDTE;

using Lardite.RefAssistant.VsProxy.Building;
using Lardite.RefAssistant.VsProxy.Projects;

namespace Lardite.RefAssistant.VsProxy
{
    /// <summary>
    /// The class encapsulates logic of interaction with Visual Studio API.
    /// </summary>
    internal sealed class VsFacade : IVsSolutionFacade, IVsProjectFacade
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Lazy<DTE> _dte;

        public VsFacade(IServiceProvider serviceProvider)
        {
            ThrowUtils.ArgumentNull(() => serviceProvider);

            _serviceProvider = serviceProvider;
            _dte = new Lazy<DTE>(() => (DTE)_serviceProvider.GetService(typeof(DTE)));
        }

        #region IVsSolutionFacade implementation

        public IVsProjectExtended GetActiveProject()
        {
            var project = GetActiveSolutionProject();
            return VsProjectMapper.Map(project);
        }

        public IVsProjectExtended GetProject(string projectName)
        {
            ThrowUtils.ArgumentNullOrEmpty(() => projectName);

            var project = GetProjectByName(projectName);
            return VsProjectMapper.Map(project);
        }

        public bool IsBuildInProgress()
        {
            return DTE.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateInProgress;
        }

        #endregion

        #region IVsProjectFacade implementation

        public BuildResult Build(string projectName)
        {
            ThrowUtils.ArgumentNullOrEmpty(() => projectName);

            string configuration = DTE.Solution.SolutionBuild.ActiveConfiguration.Name;
            Project project = GetProjectByName(projectName);

            DTE.Solution.SolutionBuild.BuildProject(
                configuration,
                project.UniqueName,
                true);

            int completionCode = DTE.Solution.SolutionBuild.LastBuildInfo;

            return new BuildResult(VsProjectMapper.Map(project), completionCode == 0);
        }

        #endregion

        #region Helpers

        private DTE DTE
        {
            get { return _dte.Value; }
        }

        private Project GetActiveSolutionProject()
        {
            Array activeProjects = (Array)DTE.ActiveSolutionProjects;

            Contract.Assert(activeProjects != null);
            Contract.Assert(activeProjects.Length == 1);

            return (Project)activeProjects.GetValue(0);
        }

        private Project GetProjectByName(string projectName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(projectName));

            // check if searched project is active
            Project activeProject = GetActiveSolutionProject();
            if (activeProject != null 
                && string.Equals(GetProjectName(activeProject), projectName, StringComparison.Ordinal))
            {
                return activeProject;
            }

            // enumerate all projects in solution
            Func<Project, IEnumerable<Project>> projectSelector = p => 
                {
                    if (p == null || p.ProjectItems == null)
                        return Enumerable.Empty<Project>();

                    return p.ProjectItems.Cast<ProjectItem>().Select(pi => pi.SubProject);
                };

            IEnumerable<Project> projects = DTE.Solution.Projects.Cast<Project>()
                .SelectMany(p => TreeWalker.Walk<Project>(p, projectSelector));

            Project project = projects.SingleOrDefault(
                p => string.Equals(GetProjectName(p), projectName, StringComparison.Ordinal));

            if (project != null)
                return project;

            throw new ProjectNotFoundException(projectName);
        }

        /// <summary>
        /// Gets name of project. Several types of projects raise exception when try to get Name property (e.g. a folder containing the project).
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns>Returns project name, otherwise empty string.</returns>
        private static string GetProjectName(Project project)
        {
            try
            {
                return project.Name;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region Nested types

        private static class TreeWalker
        {
            public static IEnumerable<T> Walk<T>(T root, Func<T, IEnumerable<T>> next)
            {
                var q = next(root).SelectMany(n => Walk(n, next));
                return Enumerable.Repeat(root, 1).Concat(q);
            }
        }  

        #endregion
    }
}
