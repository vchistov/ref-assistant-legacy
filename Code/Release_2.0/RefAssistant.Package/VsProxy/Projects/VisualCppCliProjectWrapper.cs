//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com),
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using EnvDTE;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    /// <summary>
    /// Wraps Visual C++/CLI project.
    /// </summary>
    internal class VisualCppCliProjectWrapper : BaseProjectWrapper
    {
        #region Constants

        private const string PrimaryOutput = "Primary Output";
        private const string ManagedExtensions = "ManagedExtensions";

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

        #region Properties

        /// <summary>
        /// Defines whether this project is managed c++ or native.
        /// </summary>
        public bool IsManaged
        {
            get
            {
                try
                {
                    // me is Microsoft.VisualStudio.VCProject.compileAsManagedOptions enum
                    var me = (int)Project.ConfigurationManager
                        .ActiveConfiguration.Properties.Item(ManagedExtensions).Value;
                    return me != 0; // not equals "managedNotSet"
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion // Properties

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
