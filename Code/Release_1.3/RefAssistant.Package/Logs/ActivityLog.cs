//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using System;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

using Lardite.RefAssistant.Extensions;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Writes information to visual studio activity log.
    /// </summary>
    sealed class ActivityLog : ILog
    {
        #region Constants

        private const string SourceName = "Lardite.RefAssistant";

        #endregion

        #region Fields

        private readonly IServiceProvider _serviceProvider;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="ActivityLog"/> class.
        /// </summary>
        /// <param name="serviceProvider">Package service provider.</param>
        public ActivityLog(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        #endregion // .ctor

        #region Properties

        /// <summary>
        /// Vs activity log.
        /// </summary>
        private IVsActivityLog VsActivityLog
        {
            get
            {
                return _serviceProvider.GetService(typeof(SVsActivityLog)) as IVsActivityLog;
            }
        }

        #endregion // Properties

        #region ILog Implementation

        /// <summary>
        /// Writes information.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Information(string message)
        {
            LogMessage(message, (UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION);
        }

        /// <summary>
        /// Writes warning.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Warning(string message)
        {
            LogMessage(message, (UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_WARNING);
        }

        /// <summary>
        /// Writes error.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Error(string message)
        {
            LogMessage(message, (UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR);
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

        #endregion // ILog implementation

        #region Private methods

        /// <summary>
        /// Writes message.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="messageCategory">Message category.</param>
        private void LogMessage(string message, UInt32 messageCategory)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            VsActivityLog.LogEntry(messageCategory, SourceName, string.Format(CultureInfo.CurrentCulture, message));
        }

        #endregion // Private methods
    }
}