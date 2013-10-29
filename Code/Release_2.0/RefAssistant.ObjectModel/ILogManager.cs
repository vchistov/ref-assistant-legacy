//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using System;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Log manager.
    /// </summary>
    public interface ILogManager
    {
        #region Methods

        /// <summary>
        /// Writes information.
        /// </summary>
        /// <param name="message">Message.</param>
        void Information(string message);

        /// <summary>
        /// Writes warning.
        /// </summary>
        /// <param name="message">Message.</param>
        void Warning(string message);

        /// <summary>
        /// Writes warning.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="exception">Exception.</param>
        void Warning(string message, Exception exception);

        /// <summary>
        /// Writes error.
        /// </summary>
        /// <param name="message">Message.</param>
        void Error(string message);

        /// <summary>
        /// Writes error.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="exception">Exception.</param>
        void Error(string message, Exception exception);

        #endregion
    }
}