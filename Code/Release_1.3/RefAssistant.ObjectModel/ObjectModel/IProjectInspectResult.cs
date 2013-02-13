//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// Presents the result of inspection of a project.
    /// </summary>
    public interface IProjectInspectResult
    {
        /// <summary>
        /// Information about analyzed project.
        /// </summary>
        ProjectInfo Project { get; }

        /// <summary>
        /// The list of found unused references.
        /// </summary>
        IList<ProjectReference> UnusedReferences { get; }

        /// <summary>
        /// Indicates the error was occured during analyzing.
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// The detail error information.
        /// </summary>
        Exception Exception { get; }
    }
}