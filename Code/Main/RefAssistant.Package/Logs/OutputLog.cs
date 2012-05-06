//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using System;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

using Lardite.RefAssistant.Extensions;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Writes information to visual studio output window.
    /// </summary>
    sealed class OutputLog : ILog
    {
        #region Constants

        private const string SourceName = "Lardite.RefAssistant";

        #endregion

        #region Fields

        private readonly IServiceProvider _serviceProvider;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="OutputLog"/> class.
        /// </summary>
        /// <param name="serviceProvider">Package service provider.</param>
        public OutputLog(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        #endregion // .ctor

        #region Properties

        /// <summary>
        /// Get DTE2 object.
        /// </summary>
        private DTE2 DTE2
        {
            get { return (DTE2)_serviceProvider.GetService(typeof(DTE)); }
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
            if (string.IsNullOrWhiteSpace(message) || DTE2.ToolWindows.OutputWindow.ActivePane == null)
                return;

            StringBuilder sb = new StringBuilder();
            switch (errorCategory)
            {
                case TaskErrorCategory.Warning:
                    sb.Append(SourceName).Append(" ").Append(Resources.OutputLog_Warning).Append(": ").Append(message).AppendLine(); break;
                case TaskErrorCategory.Error:
                    sb.Append(SourceName).Append(" ").Append(Resources.OutputLog_Error).Append(": ").Append(message).AppendLine(); break;
                default:
                    sb.Append(message).AppendLine(); break;
            }

            DTE2.ToolWindows.OutputWindow.ActivePane.OutputString(sb.ToString());
            DTE2.ExecuteCommand("View.Output", "");
        }

        #endregion // Private methods
    }
}