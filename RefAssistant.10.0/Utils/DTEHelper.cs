//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Lardite.RefAssistant.Utils
{
    /// <summary>
    /// Work with high-level Visual Studio's DTE object.
    /// </summary>
    static class DTEHelper
    {
        #region Constants

        private const string OutputPath = "OutputPath";
        private const string LocalPath = "LocalPath";
        private const string OutputFileName = "OutputFileName";
        private const string PrimaryOutput = "Primary Output";
        private const string FullPath = "FullPath";

        #endregion // Constants

        #region Public methods

        /// <summary>
        /// Compiles a project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="assemblyFile">Assembly path.</param>
        /// <returns>Returns zero (0) if there are no exceptions.</returns>
        public static int BuildProject(Project project, out string assemblyFile)
        {
            project.DTE.Solution.SolutionBuild.BuildProject(
                project.DTE.Solution.SolutionBuild.ActiveConfiguration.Name,
                project.UniqueName,
                true);

            assemblyFile = (project.DTE.Solution.SolutionBuild.LastBuildInfo == 0)
                ? GetOutputAssemblyPath(project) : null;

            return project.DTE.Solution.SolutionBuild.LastBuildInfo;
        }

        /// <summary>
        /// Removes unused usings from project classes.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="serviceProvider">Service provider.</param>
        public static void RemoveUnusedUsings(Project project, IServiceProvider serviceProvider)
        {
            if (Guid.Parse(project.Kind) == ProjectKinds.CSharp)
            {
                RunningDocumentTable docTable = new RunningDocumentTable(serviceProvider);
                var alreadyOpenFiles = docTable.Select(info => info.Moniker).ToList();

                string fileName;
                foreach (ProjectItem projectItem in new ProjectItemIterator(project.ProjectItems).Where(item => item.FileCodeModel != null))
                {
                    fileName = projectItem.get_FileNames(0);

                    Window window = project.DTE.OpenFile(EnvDTE.Constants.vsViewKindTextView, fileName);
                    window.Activate();

                    try
                    {
                        project.DTE.ExecuteCommand("Edit.RemoveAndSort", string.Empty);
                    }
                    catch (COMException e)
                    {
                        //Do nothing, go to the next item
                        if (LogManager.ActivityLog != null)
                            LogManager.ActivityLog.Error(null, e);
                    }

                    if (alreadyOpenFiles.SingleOrDefault(file => file.Equals(fileName, StringComparison.OrdinalIgnoreCase)) != null)
                    {
                        project.DTE.ActiveDocument.Save(fileName);
                    }
                    else
                    {
                        window.Close(vsSaveChanges.vsSaveChangesYes);
                    }
                }
            }
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
        /// <param name="project"></param>
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

            Array activeSolutionProjects = (Array)dte.ActiveSolutionProjects;
            if (activeSolutionProjects.Length == 0)
                return null;

            return (Project)activeSolutionProjects.GetValue(0);
        }

        #endregion // Public methods

        #region Private methods

        private static string GetOutputAssemblyPath(Project project)
        {
            if (Guid.Parse(project.Kind) == ProjectKinds.VisualCppCli)
            {
                var primaryOutput = project.ConfigurationManager.ActiveConfiguration.OutputGroups.Item(PrimaryOutput);
                if (primaryOutput != null && primaryOutput.FileCount > 0)
                {
                    var url = ((object[])primaryOutput.FileURLs)[0].ToString();
                    return new Uri(url).LocalPath;
                }
            }
            else if (Guid.Parse(project.Kind) == ProjectKinds.FSharp)
            {
                string outputPath = project.ConfigurationManager.ActiveConfiguration.Properties.Item(OutputPath).Value.ToString();
                string fullPath = project.Properties.Item(FullPath).Value.ToString();
                string targetName = project.Properties.Item(OutputFileName).Value.ToString();
                return Path.Combine(fullPath, Path.Combine(outputPath, targetName));
            }
            else
            {
                string outputPath = project.ConfigurationManager.ActiveConfiguration.Properties.Item(OutputPath).Value.ToString();
                string buildPath = project.Properties.Item(LocalPath).Value.ToString();
                string targetName = project.Properties.Item(OutputFileName).Value.ToString();
                return Path.Combine(buildPath, Path.Combine(outputPath, targetName));
            }

            return null;
        }

        #endregion // Private methods
    }
}
