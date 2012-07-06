//
// Copyright © 2011-2012 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//         Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;

using Lardite.RefAssistant.ObjectModel;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// The Visual Studio API facade.
    /// </summary>
    public interface IShellGateway
    {
        /// <summary>
        /// Builds project.
        /// </summary>
        /// <param name="projectInfo">The project information. If null then builds active project.</param>
        /// <returns>Returns true if success; otherwise false.</returns>
        CompilationInfo BuildProject(ProjectInfo projectInfo);

        /// <summary>
        /// Builds current solution.
        /// </summary>        
        /// <returns>Returns true if success; otherwise false.</returns>
        bool BuildSolution();

        /// <summary>
        /// Gets active project info.
        /// </summary>
        /// <returns>Active project info.</returns>
        ProjectInfo GetActiveProjectInfo();

        /// <summary>
        /// Gets project info by name.
        /// </summary>
        /// <param name="projectName">The project name.</param>
        /// <returns>Returns project info.</returns>
        ProjectInfo GetProjectInfo(string projectName);

        /// <summary>
        /// Get information about solution's projects.
        /// </summary>
        /// <param name="unsupportedProjects">The list of unsupported projects.</param>
        /// <returns>Returns list of projects.</returns>
        IEnumerable<ProjectInfo> GetSolutionProjects(out IEnumerable<string> unsupportedProjects);

        /// <summary>
        /// Can show unused references window.
        /// </summary>        
        bool IsRemovingConfirmationRequired { get; }

        /// <summary>
        /// Shows unused references window to confirm removing.
        /// </summary>
        /// <param name="inspectResults">Unused project references.</param>
        /// <returns>If true, then continue.</returns>
        bool ConfirmUnusedReferencesRemoving(IInspectResult inspectResults);

        /// <summary>
        /// Can remove unused references.
        /// </summary>
        /// <param name="projectName">The project name. If null then gets active project.</param>
        bool CanRemoveUnusedReferences(string projectName);

        /// <summary>
        /// Removes unused references.
        /// </summary>
        /// <param name="inspectResults">Unused project references.</param>
        /// <returns>Removed references count.</returns>
        void RemoveUnusedReferences(IInspectResult inspectResults);

        /// <summary>
        /// Can remove unused using. Checks only settings.
        /// </summary>        
        /// <returns>If true, then can.</returns>
        bool CanRemoveUnusedUsings();

        /// <summary>
        /// Can remove unused using.
        /// </summary>
        /// <param name="projectName">The project name. If null then gets active project.</param>
        /// <returns>If true, then can.</returns>
        bool CanRemoveUnusedUsings(string projectName);

        /// <summary>
        /// Removes unused usings.
        /// </summary>
        /// <param name="projectInfo">The project information. If null then gets active project.</param>
        void RemoveUnusedUsings(ProjectInfo projectInfo);
    }
}
