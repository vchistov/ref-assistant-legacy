//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lardite.RefAssistant.ObjectModel
{
    internal sealed class ProjectInspectResult : IProjectInspectResult
    {        
        #region .ctor

        public ProjectInspectResult(ProjectInfo projectInfo, IEnumerable<ProjectReference> unusedReferences)
            : this(projectInfo, unusedReferences, null)
        {
        }

        public ProjectInspectResult(ProjectInfo projectInfo, Exception exception)
            : this(projectInfo, null, exception)
        {
        }

        private ProjectInspectResult(ProjectInfo projectInfo, IEnumerable<ProjectReference> unusedReferences, Exception exception)
        {
            this.Project = projectInfo;
            this.Exception = exception;

            this.UnusedReferences = (exception == null && unusedReferences != null) 
                ? unusedReferences.ToList()
                : new List<ProjectReference>();
        }

        #endregion // .ctor

        #region IProjectInspectResult implementation

        public ProjectInfo Project { get; private set; }

        public IList<ProjectReference> UnusedReferences { get; private set; }

        public bool IsSuccess
        {
            get { return this.Exception == null; }
        }

        public Exception Exception { get; private set; }

        #endregion // IProjectInspectResult implementation
    }
}
