using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Utility for throwing exceptions. This class is shared between projects.
    /// </summary>
    internal static class ThrowUtils
    {
        [DebuggerStepThrough]
        public static void ArgumentNull<T>(Expression<Func<T>> param) where T : class
        {
            var paramValue = GetParameter(param);
            if (paramValue.Value == null)
            {
                throw new ArgumentNullException(paramValue.Key);
            }
        }

        [DebuggerStepThrough]
        public static void ArgumentNullOrEmpty(Expression<Func<string>> param)
        {
            var paramValue = GetParameter(param);
            if (string.IsNullOrWhiteSpace(paramValue.Value))
            {
                throw new ArgumentNullException(paramValue.Key);
            }
        }

        [DebuggerStepThrough]
        public static void InvalidOperation(string message)
        {
            throw new InvalidOperationException(message);
        }

        #region Helpers

        private static string GetParameterName<T>(Expression<Func<T>> expression)
        {
            Debug.Assert(expression.Body is MemberExpression, "Only MemberExpression is allowed.");

            var me = expression.Body as MemberExpression;
            return me.Member.Name;
        }

        private static KeyValuePair<string, T> GetParameter<T>(Expression<Func<T>> expression)
        {
            Debug.Assert(expression.Body is MemberExpression, "Only MemberExpression is allowed.");

            var me = expression.Body as MemberExpression;
            T value = (expression.Compile())();
            return new KeyValuePair<string, T>(me.Member.Name, value);
        }

        #endregion // Helpers
    }
}
