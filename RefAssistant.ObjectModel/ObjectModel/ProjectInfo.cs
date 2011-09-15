//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// Contains project information.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{Name}")]
    public sealed class ProjectInfo
    {
        #region Properties

        /// <summary>
        /// Get or set project name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get or set project type.
        /// </summary>
        public Guid Type { get; set; }

        /// <summary>
        /// Get or set project configuration name.
        /// </summary>
        public string ConfigurationName { get; set; }

        /// <summary>
        /// Get or set project platform name.
        /// </summary>
        public string PlatformName { get; set; }

        /// <summary>
        /// Get or set project assembly path.
        /// </summary>
        public string AssemblyPath { get; set; }

        /// <summary>
        /// Get or set project references list.
        /// </summary>
        public IEnumerable<ProjectReference> References { get; set; }

        #endregion // Properties
    }
}