//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using System;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Log manager.
    /// </summary>
    public sealed class LogManager : ILogManager
    {
        private ErrorListLog _errorListLog;
        private OutputLog _outputLog;

        private static readonly LogManager _instance;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static LogManager()
        {
            _instance = new LogManager();
        }

        #region Properties

        /// <summary>
        /// Instance of log manager.
        /// </summary>
        public static LogManager Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes log manager.
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/></param>
        public void Initialize(IServiceProvider serviceProvider)
        {
            ThrowUtils.ArgumentNull(() => serviceProvider);

            _errorListLog = new ErrorListLog(serviceProvider);
            _outputLog = new OutputLog(serviceProvider);
        }

        /// <summary>
        /// Writes information.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Information(string message)
        {
            if (_outputLog != null)
                _outputLog.Information(message);
        }

        /// <summary>
        /// Writes warning.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Warning(string message)
        {
            if (_outputLog != null)
                _outputLog.Warning(message);
        }

        /// <summary>
        /// Writes warning.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Warning(string message, Exception exception)
        {
            if (_outputLog != null)
                _outputLog.Warning(message, exception);
        }

        /// <summary>
        /// Writes error.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Error(string message)
        {
            if (_outputLog != null)
                _outputLog.Error(message);
            if (_errorListLog != null)
                _errorListLog.Error(message);
        }

        /// <summary>
        /// Writes error.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="exception">Exception.</param>
        public void Error(string message, Exception exception)
        {
            if (_outputLog != null)
                _outputLog.Error(message, exception);
            if (_errorListLog != null)
                _errorListLog.Error(message);            
        }

        #endregion
    }
}