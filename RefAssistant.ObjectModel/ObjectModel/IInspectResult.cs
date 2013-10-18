//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// The interface for passing unused references of collection projects.
    /// </summary>
    public interface IInspectResult
    {
        /// <summary>
        /// Gets the inspection result for project.
        /// </summary>
        IProjectInspectResult Result { get; }

        /// <summary>
        /// Returns true if there is unused reference for project.
        /// </summary>
        bool HasUnusedReferences { get; }
    }
}