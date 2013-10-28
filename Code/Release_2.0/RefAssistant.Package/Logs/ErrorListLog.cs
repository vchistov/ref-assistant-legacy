//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using System;
using Microsoft.VisualStudio.Shell;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Writes information to visual studio error list.
    /// </summary>
    internal sealed class ErrorListLog
    {
        private ErrorListProvider _errorListProvider;

        /// <summary>
        /// Initialize a new instance of the <see cref="ErrorListLog"/> class.
        /// </summary>
        /// <param name="serviceProvider">Package service provider.</param>
        public ErrorListLog(IServiceProvider serviceProvider)
        {
            ThrowUtils.ArgumentNull(() => serviceProvider);

            _errorListProvider = new ErrorListProvider(serviceProvider);
        }

        /// <summary>
        /// Writes error.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Error(string message)
        {
            _errorListProvider.Tasks.Clear();

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            ErrorTask errorTask = new ErrorTask()
            {
                CanDelete = false,
                ErrorCategory = TaskErrorCategory.Error,
                Text = message
            };

            _errorListProvider.Tasks.Add(errorTask);
            _errorListProvider.Show();
        }
    }
}