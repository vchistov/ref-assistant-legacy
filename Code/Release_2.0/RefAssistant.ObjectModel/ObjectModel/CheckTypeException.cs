//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// Exception happened during investigation of a some Type.
    /// </summary>
    [Serializable]
    public sealed class CheckTypeException : ApplicationException
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTypeException"/> class.
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly-qualified name of the Type, which includes the name of the assembly from which the Type was loaded, or null if the current instance represents a generic type parameter.</param>        
        public CheckTypeException(string assemblyQualifiedName)
            : this(assemblyQualifiedName, string.Format(Resources.CheckTypeException_Message, assemblyQualifiedName), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTypeException"/> class.
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly-qualified name of the Type, which includes the name of the assembly from which the Type was loaded, or null if the current instance represents a generic type parameter.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>        
        public CheckTypeException(string assemblyQualifiedName, string message)
            : this(assemblyQualifiedName, message, null)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTypeException"/> class.
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly-qualified name of the Type, which includes the name of the assembly from which the Type was loaded, or null if the current instance represents a generic type parameter.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public CheckTypeException(string assemblyQualifiedName, string message, Exception innerException)
            : base(string.IsNullOrEmpty(message) ? string.Format(Resources.CheckTypeException_Message, assemblyQualifiedName) : message, innerException)            
        {
            AssemblyQualifiedName = assemblyQualifiedName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTypeException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The serialization context.</param>
        private CheckTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion // .ctor

        #region Properties

        /// <summary>
        /// Get the assembly-qualified name of the Type, which includes the name of the assembly from which the Type was loaded, or null if the current instance represents a generic type parameter.
        /// </summary>
        public string AssemblyQualifiedName { get; private set; }

        #endregion // Properties

        #region Overrides

        /// <summary>
        /// Sets the SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion // Overrides
    }
}
