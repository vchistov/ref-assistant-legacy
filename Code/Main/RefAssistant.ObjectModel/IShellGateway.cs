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
    /// Shell gateway.
    /// </summary>
    public interface IShellGateway
    {
        /// <summary>
        /// Gets active project info.
        /// </summary>
        /// <returns>Active project info.</returns>
        ProjectInfo GetActiveProjectInfo();

        /// <summary>
        /// Shows unused references window.
        /// </summary>
        /// <param name="unusedProjectReferences">Unused project references.</param>
        /// <returns>If true, then continue.</returns>
        bool ShowUnusedReferencesWindow(ref IEnumerable<ProjectReference> unusedProjectReferences);

        /// <summary>
        /// Can show unused references window.
        /// </summary>
        /// <returns>If true, then can.</returns>
        bool CanShowUnusedReferencesWindow { get; }

        /// <summary>
        /// Removes unused references.
        /// </summary>
        /// <param name="unusedProjectReferences">Unused project references.</param>
        /// <returns>Removed references count.</returns>
        int RemoveUnusedReferences(IEnumerable<ProjectReference> unusedProjectReferences);

        /// <summary>
        /// Can remove unused references.
        /// </summary>
        bool CanRemoveUnusedReferences { get; }

        /// <summary>
        /// Can remove unused using.
        /// </summary>
        /// <returns>If true, then can.</returns>
        bool CanRemoveUnusedUsings { get; }

        /// <summary>
        /// Removes unused usings.
        /// </summary>
        void RemoveUnusedUsings();
    }
}