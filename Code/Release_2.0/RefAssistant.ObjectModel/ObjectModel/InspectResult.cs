//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;
using System.Linq;

namespace Lardite.RefAssistant.ObjectModel
{
    internal sealed class InspectResult : IInspectResult
    {
        #region Fields

        private readonly List<IProjectInspectResult> _inspectResults;

        #endregion // Fields

        public InspectResult(IEnumerable<IProjectInspectResult> results = null)
        {
            _inspectResults = new List<IProjectInspectResult>();

            if (results != null)
            {
                _inspectResults.AddRange(results);
            }
        }

        #region Public methods

        /// <summary>
        /// Add result of inspection of a project.
        /// </summary>
        /// <param name="result"></param>
        public void AddResult(IProjectInspectResult result)
        {
            if (result == null)
            {
                throw Error.ArgumentNull("result");
            }

            if (_inspectResults.Any(p => p.Project.Equals(result.Project)))
            {
                throw Error.InvalidOperation(Resources.InspectResult_ResultAlreadyAdded, result.Project.Name);
            }

            _inspectResults.Add(result);
        }

        #endregion // Public methods

        #region InspectResults implementation

        /// <summary>
        /// Gets the list of inspection results for projects.
        /// </summary>
        public IEnumerable<IProjectInspectResult> InspectResults
        {
            get { return _inspectResults; }
        }

        /// <summary>
        /// Returns true if there is unused reference for any project.
        /// </summary>
        public bool HasUnusedReferences
        {
            get { return _inspectResults.Any(p => p.UnusedReferences.Any()); }
        }

        #endregion
    }
}
