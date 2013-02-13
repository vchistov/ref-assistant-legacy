//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using System;
using System.Text;
using Microsoft.VisualStudio.Shell;

using Lardite.RefAssistant.Extensions;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Writes information to visual studio error list.
    /// </summary>
    sealed class ErrorListLog : ILog
    {
        #region Fields

        private readonly IServiceProvider _serviceProvider;
        private ErrorListProvider _errorListProvider;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="ErrorListLog"/> class.
        /// </summary>
        /// <param name="serviceProvider">Package service provider.</param>
        public ErrorListLog(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        #endregion // .ctor

        #region Properties

        /// <summary>
        /// Error list provider.
        /// </summary>
        private ErrorListProvider ErrorListProvider
        {
            get
            {
                return _errorListProvider ?? (_errorListProvider = new ErrorListProvider(_serviceProvider));
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
            LogMessage(message, TaskErrorCategory.Message);
        }

        /// <summary>
        /// Writes warning.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Warning(string message)
        {
            LogMessage(message, TaskErrorCategory.Warning);
        }

        /// <summary>
        /// Writes error.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Error(string message)
        {
            LogMessage(message, TaskErrorCategory.Error);
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
        /// <param name="errorCategory">Error category.</param>
        private void LogMessage(string message, TaskErrorCategory errorCategory)
        {
            ErrorListProvider.Tasks.Clear();
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            ErrorTask errorTask = new ErrorTask()
            {
                CanDelete = false,
                ErrorCategory = errorCategory,
                Text = message
            };

            ErrorListProvider.Tasks.Add(errorTask);
            ErrorListProvider.Show();
        }

        #endregion // Private methods
    }
}