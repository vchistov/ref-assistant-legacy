//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// The interface for passing unused references of collection projects.
    /// </summary>
    public interface IInspectResult
    {
        /// <summary>
        /// Gets the list of inspection results for projects.
        /// </summary>
        IEnumerable<IProjectInspectResult> InspectResults { get; }

        /// <summary>
        /// Returns true if there is unused reference for any project.
        /// </summary>
        bool HasUnusedReferences { get; }
    }
}