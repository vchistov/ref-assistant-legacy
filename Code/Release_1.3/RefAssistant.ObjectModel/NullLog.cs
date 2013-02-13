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
    /// Null log.
    /// </summary>
    public sealed class NullLog : ILog
    {
        #region ILog implementation

        /// <summary>
        /// Writes information.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Information(string message)
        {
        }

        /// <summary>
        /// Writes warning.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Warning(string message)
        {
        }

        /// <summary>
        /// Writes error.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Error(string message)
        {
        }

        /// <summary>
        /// Writes error.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="exception">Exception.</param>
        public void Error(string message, Exception exception)
        {
        }

        #endregion // ILog implementation
    }
}