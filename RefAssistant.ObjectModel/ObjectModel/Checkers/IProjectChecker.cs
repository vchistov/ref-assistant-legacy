//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Check project assembly types.
    /// </summary>
    interface IProjectChecker
    {
        /// <summary>
        /// Checks project references in order to find unused.
        /// </summary>
        /// <returns>Returns list of unused references.</returns>
        IEnumerable<ProjectReference> Check();
    }
}
