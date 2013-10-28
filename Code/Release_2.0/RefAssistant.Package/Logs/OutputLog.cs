//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using System;
using System.Collections;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Lardite.RefAssistant.Extensions;
using Microsoft.VisualStudio.Shell;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Writes information to visual studio output window.
    /// </summary>
    internal sealed class OutputLog
    {
        private const string WindowPaneName = "Reference Assistant";

        private readonly Lazy<OutputWindowPane> _outputPane;

        /// <summary>
        /// Initialize a new instance of the <see cref="OutputLog"/> class.
        /// </summary>
        /// <param name="serviceProvider">Package service provider.</param>
        public OutputLog(IServiceProvider serviceProvider)
        {
            ThrowUtils.ArgumentNull(() => serviceProvider);

            _outputPane = new Lazy<OutputWindowPane>(() => GetOutputPane(serviceProvider));
        }

        #region Public methods

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

        #endregion

        #region Private methods

        private OutputWindowPane OutputPane
        {
            get { return _outputPane.Value; }
        }

        private void LogMessage(string message, TaskErrorCategory errorCategory)
        {
            if (string.IsNullOrWhiteSpace(message) || this.OutputPane == null)
                return;

            StringBuilder sb = new StringBuilder();
            switch (errorCategory)
            {
                case TaskErrorCategory.Warning:
                    sb.Append(Resources.OutputLog_Warning).Append(": ").Append(message).AppendLine(); break;
                case TaskErrorCategory.Error:
                    sb.Append(Resources.OutputLog_Error).Append(": ").Append(message).AppendLine(); break;
                default:
                    sb.Append(message).AppendLine(); break;
            }

            this.OutputPane.OutputString(sb.ToString());
            this.OutputPane.Activate();
        }

        private OutputWindowPane GetOutputPane(IServiceProvider serviceProvider)
        {
            var dte = (DTE2)serviceProvider.GetService(typeof(DTE));

            IEnumerator windowsEnumerator = dte.ToolWindows.OutputWindow.OutputWindowPanes.GetEnumerator();
            while (windowsEnumerator.MoveNext())
            {
                OutputWindowPane windowPane = (OutputWindowPane)windowsEnumerator.Current;
                if (string.Equals(WindowPaneName, windowPane.Name, StringComparison.Ordinal))
                {
                    return windowPane;
                }
            }

            return dte.ToolWindows.OutputWindow.OutputWindowPanes.Add(WindowPaneName);
        }

        #endregion
    }
}