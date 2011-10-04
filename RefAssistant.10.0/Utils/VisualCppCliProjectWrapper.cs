//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com),
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using EnvDTE;

namespace Lardite.RefAssistant.Utils
{
    /// <summary>
    /// Wraps Visual C++/CLI project.
    /// </summary>
    internal class VisualCppCliProjectWrapper : BaseProjectWrapper
    {
        #region Constants

        private const string PrimaryOutput = "Primary Output";

        #endregion // Constants

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="VisualCppCliProjectWrapper"/> class.
        /// </summary>
        /// <param name="vsProject">The Visual Studio project.</param>
        public VisualCppCliProjectWrapper(Project vsProject)
            : base(vsProject)
        { }

        #endregion // .ctor

        #region Public methods

        /// <summary>
        /// Get outout assembly path.
        /// </summary>
        /// <returns>Returns full path.</returns>
        public override string GetOutputAssemblyPath()
        {
            var primaryOutput = Project.ConfigurationManager.ActiveConfiguration.OutputGroups.Item(PrimaryOutput);
            if (primaryOutput != null && primaryOutput.FileCount > 0)
            {
                var url = ((object[])primaryOutput.FileURLs)[0].ToString();
                return new Uri(url).LocalPath;
            }
            return null;
        }

        #endregion // Public methods
    }
}
