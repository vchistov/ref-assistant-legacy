//
// Copyright © 2011 Lardite.
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
        /// Get information about solution's projects.
        /// </summary>
        /// <returns>Returns list of projects.</returns>
        IEnumerable<ProjectInfo> GetSolutionProjectsInfo();

        /// <summary>
        /// Can show unused references window.
        /// </summary>        
        bool CanShowUnusedReferencesWindow();

        /// <summary>
        /// Shows unused references window.
        /// </summary>
        /// <param name="unusedProjectReferences">Unused project references.</param>
        /// <returns>If true, then continue.</returns>
        bool ShowUnusedReferencesWindow(ref IEnumerable<ProjectReference> unusedProjectReferences);

        /// <summary>
        /// Can remove unused references.
        /// </summary>
        /// <param name="projectInfo">The project information. If null then gets active project.</param>
        bool CanRemoveUnusedReferences(ProjectInfo projectInfo);

        /// <summary>
        /// Removes unused references.
        /// </summary>
        /// <param name="projectInfo">The project information. If null then gets active project.</param>
        /// <param name="unusedProjectReferences">Unused project references.</param>
        /// <returns>Removed references count.</returns>
        int RemoveUnusedReferences(ProjectInfo projectInfo, IEnumerable<ProjectReference> unusedProjectReferences);
        
        /// <summary>
        /// Can remove unused using.
        /// </summary>
        /// <param name="projectInfo">The project information. If null then gets active project.</param>
        /// <returns>If true, then can.</returns>
        bool CanRemoveUnusedUsings(ProjectInfo projectInfo);

        /// <summary>
        /// Removes unused usings.
        /// </summary>
        /// <param name="projectInfo">The project information. If null then gets active project.</param>
        void RemoveUnusedUsings(ProjectInfo projectInfo);
    }
}
