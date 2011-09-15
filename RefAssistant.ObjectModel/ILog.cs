//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//         Chistov Victor (vchistov@lardite.com)
//

using System;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Log.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Write information message.
        /// </summary>
        /// <param name="message">The Message.</param>
        void Information(string message);

        /// <summary>
        /// Write warning message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Warning(string message);

        /// <summary>
        /// Write error message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Error(string message);

        /// <summary>
        /// Write occured exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="message">The exception.</param>
        void Error(string message, Exception exception);
    }
}