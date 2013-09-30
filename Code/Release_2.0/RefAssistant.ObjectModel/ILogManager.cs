//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Log manager.
    /// </summary>
    public interface ILogManager
    {
        #region Properties

        /// <summary>
        /// Activity log.
        /// </summary>
        ILog ActivityLog { get; set; }

        /// <summary>
        /// Error list log.
        /// </summary>
        ILog ErrorListLog { get; set; }

        /// <summary>
        /// Output log.
        /// </summary>
        ILog OutputLog { get; set; }

        #endregion // Properties
    }
}