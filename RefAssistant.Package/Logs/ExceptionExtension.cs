//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections;
using System.Text;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Extentions of the <see cref="System.Exception"/> class.
    /// </summary>
    public static class ExceptionExtension
    {
        #region Public methods

        /// <summary>
        /// Creates and returns a string representation of the current exception with Data content.
        /// </summary>
        /// <param name="exception">An exception.</param>
        /// <returns>A string representation of the current exception.</returns>
        public static string ToTraceString(this Exception exception)
        {
            var sb = new StringBuilder(exception.ToString());

            if (exception.Data.Count > 0)
            {
                sb.AppendLine().AppendLine("Exception Data:");
                foreach (DictionaryEntry data in exception.Data)
                {
                    sb.AppendFormat("  {0}:{1}", data.Key, data.Value).AppendLine();
                }
            }

            return sb.ToString();
        }

        #endregion // Public methods
    }
}