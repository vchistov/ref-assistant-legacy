//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Lardite.RefAssistant.Utils
{
    /// <summary>
    /// Work with high-level Visual Studio's DTE object.
    /// </summary>
    static class DTEHelper
    {
        #region Public methods

        /// <summary>
        /// Compiles a project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns>Returns zero (0) if there are no exceptions.</returns>
        public static int BuildProject(Project project)
        {
            project.DTE.Solution.SolutionBuild.BuildProject(
                project.DTE.Solution.SolutionBuild.ActiveConfiguration.Name,
                project.UniqueName,
                true);

            return project.DTE.Solution.SolutionBuild.LastBuildInfo;
        }

        /// <summary>
        /// Compiles a solution.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>Returns zero (0) if there are no exceptions.</returns>
        public static int BuildSolution(IServiceProvider serviceProvider)
        {
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));

            Solution solution = dte.Solution;
            solution.SolutionBuild.Build(true);

            return solution.SolutionBuild.LastBuildInfo;
        }

        /// <summary>
        /// Creates wrapper for visual studio project.
        /// </summary>
        /// <param name="project">Visual Studio project.</param>
        /// <returns>Returns wrapper.</returns>
        public static BaseProjectWrapper CreateProjectWrapper(Project project)
        {
            if (Guid.Parse(project.Kind) == ProjectKinds.FSharp)
            {
                return new FSharpProjectWrapper(project);
            }
            else if (Guid.Parse(project.Kind) == ProjectKinds.VisualCppCli)
            {
                return new VisualCppCliProjectWrapper(project);
            }
            else if (Guid.Parse(project.Kind) == ProjectKinds.CSharp)
            {
                return new CSharpProjectWrapper(project);
            }

            // default wrapper
            return new BaseProjectWrapper(project);
        }

        /// <summary>
        /// Shows error list.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        public static void ShowErrorList(IServiceProvider serviceProvider)
        {
            using (var errorList = new ErrorListProvider(serviceProvider))
            {
                errorList.Show();
            }
        }

        /// <summary>
        /// Check wherether project is building currently.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns>Returns True if project is building currently.</returns>
        public static bool IsBuildInProgress(Project project)
        {
            return project.DTE.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateInProgress;
        }

        /// <summary>
        /// Get active project in solution.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <returns>Returns the active project.</returns>
        public static Project GetActiveProject(IServiceProvider serviceProvider)
        {
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            return GetActiveSolutionProject(dte);
        }

        /// <summary>
        /// Get active project in solution.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <returns>Returns the active project.</returns>
        public static Project GetProjectByName(IServiceProvider serviceProvider, string projectName)
        {
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            
            // check if searched project is active
            var activeProject = GetActiveSolutionProject(dte);
            if (activeProject != null && activeProject.Name.Equals(projectName, StringComparison.Ordinal))
            {
                return activeProject;
            }

            // enumerate all projects in solution
            var enumerator = dte.Solution.Projects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var project = SearchProjectByName(enumerator.Current as Project, projectName);
                if (project != null)
                {
                    return project;
                }
            }
            return null;
        }

        #endregion // Public methods

        #region Private methods

        /// <summary>
        /// Recursive search of project by name.
        /// </summary>
        /// <param name="project">The Visual Studion project.</param>
        /// <param name="name">The name of project.</param>
        /// <returns>Returns Project if found, otherwise null.</returns>
        private static Project SearchProjectByName(Project project, string name)
        {
            if (project == null)
            {
                return null;
            }

            if (project.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return project;
            }

            var enumerator = project.ProjectItems.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var projectItem = (ProjectItem)enumerator.Current;
                var findedProject = SearchProjectByName(projectItem.SubProject, name);
                if (findedProject != null)
                {
                    return findedProject;
                }
            }

            // found nothing
            return null;
        }

        /// <summary>
        /// Get solution's active project.
        /// </summary>
        /// <param name="dte"></param>
        /// <returns></returns>
        private static Project GetActiveSolutionProject(DTE dte)
        {
            Array activeSolutionProjects = (Array)dte.ActiveSolutionProjects;
            if (activeSolutionProjects.Length == 0)
                return null;

            return (Project)activeSolutionProjects.GetValue(0);
        }

        #endregion // Private methods
    }
}
