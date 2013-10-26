//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using System;
using System.Text;

using Lardite.RefAssistant.Extensions;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Writes information to visual studio activity log.
    /// </summary>
    sealed class ActivityLog : ILog
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("ReferenceAssistant");

        #region ILog Implementation

        /// <summary>
        /// Writes information.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Information(string message)
        {
            log.Info(message);
        }

        /// <summary>
        /// Writes warning.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Warning(string message)
        {
            log.Warn(message);
        }

        /// <summary>
        /// Writes error.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Error(string message)
        {
            log.Error(message);
        }

        /// <summary>
        /// Writes error.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="exception">Exception.</param>
        public void Error(string message, Exception exception)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(message))
            {
                sb.Append(message);
            }

            if (exception != null)
            {
                if (sb.Length > 0)
                    sb.AppendLine();
                sb.Append(exception.ToTraceString());
            }

            Error(sb.ToString());
        }

        #endregion
    }
}