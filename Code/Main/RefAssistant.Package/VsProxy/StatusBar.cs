//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace Lardite.RefAssistant.VsProxy
{
    internal class StatusBar
    {
        #region Fields

        private object StatusBarIconID = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_General;

        private readonly IServiceProvider _serviceProvider;
        private readonly Lazy<IVsStatusbar> _statusBar;

        #endregion // Fields

        #region .ctor
        
        /// <summary>
        /// Initialize a new instance of the <see cref="StatusBar"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public StatusBar(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull("serviceProvider");
            }

            _serviceProvider = serviceProvider;
            _statusBar = new Lazy<IVsStatusbar>(() => (IVsStatusbar)_serviceProvider.GetService(typeof(SVsStatusbar)));
        }

        #endregion // ctor
                
        #region Public methods

        /// <summary>
        /// Starts status bar animation.
        /// </summary>
        public void StartStatusBarAnimation()
        {
            try
            {
                _statusBar.Value.SetText(string.Empty);
                _statusBar.Value.Animation(1, ref StatusBarIconID);
            }
            catch (Exception ex)
            {
                LogManager.ErrorListLog.Error(string.Empty, ex);
            }
        }

        /// <summary>
        /// Stops status bar animation.
        /// </summary>
        public void StopStatusBarAnimation()
        {
            try
            {
                _statusBar.Value.Animation(0, ref StatusBarIconID);
            }
            catch (Exception ex)
            {
                LogManager.ErrorListLog.Error(string.Empty, ex);
            }
        }

        /// <summary>
        /// Sets status bar text.
        /// </summary>
        /// <param name="text">Text.</param>
        public void SetStatusBarText(string text)
        {
            try
            {
                _statusBar.Value.SetText(text);
            }
            catch (Exception ex)
            {
                LogManager.ErrorListLog.Error(string.Empty, ex);
            }
        }

        #endregion // Public methods
    }
}
