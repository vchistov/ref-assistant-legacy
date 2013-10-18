//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Linq;

namespace Lardite.RefAssistant.ObjectModel
{
    internal sealed class InspectResult : IInspectResult
    {
        private readonly IProjectInspectResult _inspectResult;

        public InspectResult(IProjectInspectResult inspectResult)
        {
            _inspectResult = inspectResult;
        }        

        #region InspectResults implementation

        /// <summary>
        /// Gets inspection result for project.
        /// </summary>
        public IProjectInspectResult Result
        {
            get 
            { 
                return _inspectResult; 
            }
        }

        /// <summary>
        /// Returns true if there is unused reference for project.
        /// </summary>
        public bool HasUnusedReferences
        {
            get 
            {
                return _inspectResult.UnusedReferences.Any(); 
            }
        }

        #endregion
    }
}