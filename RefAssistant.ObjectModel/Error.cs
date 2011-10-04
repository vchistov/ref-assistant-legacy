//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//

using System;
using System.Globalization;
using System.IO;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Exception helper.
    /// </summary>
    public static class Error
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="System.ObjectDisposedException"/> class.
        /// </summary>
        /// <param name="obj">A string containing the name of the disposed object.</param>
        public static Exception ObjectDisposed(object obj)
        {
            return new ObjectDisposedException(obj.GetType().Name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.ArgumentNullException"/> class.
        /// </summary>
        /// <param name="paramName">The name of the parameter that caused the exception.</param>
        public static Exception ArgumentNull(string paramName)
        {
            return new ArgumentNullException(paramName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.ArgumentNullException"/> class.
        /// </summary>
        /// <param name="paramName">The name of the parameter that caused the exception.</param>
        /// <param name="message">The error message.</param>
        public static Exception ArgumentNull(string paramName, string message)
        {
            return new ArgumentNullException(paramName, message);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.ArgumentOutOfRangeException"/> class.
        /// </summary>
        /// <param name="paramName">The name of the parameter that caused the exception.</param>
        /// <param name="actualValue">The value of the argument that causes this exception.</param>
        public static Exception ArgumentOutOfRange(string paramName, object actualValue)
        {
            return new ArgumentOutOfRangeException(paramName, actualValue, Resources.Error_ArgumentOutOfRange);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.ArgumentException"/> class.
        /// </summary>
        /// <param name="paramName">The name of the parameter that caused the exception.</param>
        public static Exception Argument(string paramName)
        {
            return new ArgumentException(string.Format(Resources.Error_ArgumentEmpty, paramName), paramName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.NotSupportedException"/> class.
        /// </summary>  
        public static Exception NotSupported()
        {
            return new NotSupportedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.NotSupportedException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="args">Parameters.</param>
        public static Exception NotSupported(string message, params object[] args)
        {
            return new NotSupportedException(string.Format(CultureInfo.CurrentCulture, message, args));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.NotImplementedException"/> class.
        /// </summary>
        public static Exception NotImplemented()
        {
            return new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.InvalidOperationException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public static Exception InvalidOperation(string message)
        {
            return new InvalidOperationException(message);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.InvalidOperationException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="template">String template.</param>
        /// <param name="parameters">Parameters.</param>
        public static Exception InvalidOperation(string template, params object[] parameters)
        {
            return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, template, parameters));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.FileNotFoundException"/> class.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public static Exception FileNotFound(string fileName)
        {
            return new FileNotFoundException(fileName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.InvalidCastException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public static Exception InvalidCast(string message)
        {
            return new InvalidCastException(message);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lardite.RefAssistant.ObjectModel.CheckTypeException"/> class.
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly-qualified name of the Type.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="exception">Occured exception.</param>
        /// <returns>Returns <see cref="Lardite.RefAssistant.ObjectModel.CheckTypeException"/> object.</returns>
        public static Exception CheckType(string assemblyQualifiedName, string message, Exception exception)
        {
            return new ObjectModel.CheckTypeException(assemblyQualifiedName, message, exception);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lardite.RefAssistant.ObjectModel.CheckTypeException"/> class.
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly-qualified name of the Type.</param>        
        /// <param name="exception">Occured exception.</param>
        /// <returns>Returns <see cref="Lardite.RefAssistant.ObjectModel.CheckTypeException"/> object.</returns>
        public static Exception CheckType(string assemblyQualifiedName, Exception exception)
        {
            return new ObjectModel.CheckTypeException(assemblyQualifiedName, null, exception);
        }
    }
}
