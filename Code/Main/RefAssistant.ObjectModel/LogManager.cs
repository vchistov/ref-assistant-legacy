//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using Lardite.RefAssistant;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Log manager.
    /// </summary>
    public sealed class LogManager : ILogManager
    {
        #region Fields

        private readonly ILog _nullLog;
        private ILog _activityLog;
        private ILog _errorListLog;
        private ILog _outputLog;
        private static readonly LogManager _instance;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Constructor.
        /// </summary>
        private LogManager()
        {
            _nullLog = new NullLog();
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static LogManager()
        {
            _instance = new LogManager();
        }

        #endregion // .ctor

        #region Properties

        /// <summary>
        /// Activity log.
        /// </summary>
        public static ILog ActivityLog
        {
            get { return _instance._activityLog ?? _instance._nullLog; }
            set { _instance._activityLog = value ?? _instance._nullLog; }
        }

        /// <summary>
        /// Error list log.
        /// </summary>
        public static ILog ErrorListLog
        {
            get { return _instance._errorListLog ?? _instance._nullLog; }
            set { _instance._errorListLog = value ?? _instance._nullLog; }
        }

        /// <summary>
        /// Output window log.
        /// </summary>
        public static ILog OutputLog
        {
            get { return _instance._outputLog ?? _instance._nullLog; }
            set { _instance._outputLog = value ?? _instance._nullLog; }
        }

        #endregion // Properties

        #region ILogManager Implementation

        /// <summary>
        /// Activity log.
        /// </summary>
        ILog ILogManager.ActivityLog
        {
            get { return ActivityLog; }
            set { ActivityLog = value; }
        }

        /// <summary>
        /// Error list log.
        /// </summary>
        ILog ILogManager.ErrorListLog
        {
            get { return ErrorListLog; }
            set { ErrorListLog = value; }
        }

        /// <summary>
        /// Output window log.
        /// </summary>
        ILog ILogManager.OutputLog
        {
            get { return OutputLog; }
            set { OutputLog = value; }
        }

        #endregion // ILogManager Implementation
    }
}