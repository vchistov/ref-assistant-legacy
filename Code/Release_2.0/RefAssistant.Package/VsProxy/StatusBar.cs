//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace Lardite.RefAssistant.VsProxy
{
    internal sealed class StatusBar
    {
        private object StatusBarIconID = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Find;
        private readonly Lazy<IVsStatusbar> _statusBar;

        /// <summary>
        /// Initialize a new instance of the <see cref="StatusBar"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public StatusBar(IServiceProvider serviceProvider)
        {
            ThrowUtils.ArgumentNull(() => serviceProvider);

            _statusBar = new Lazy<IVsStatusbar>(() => (IVsStatusbar)serviceProvider.GetService(typeof(SVsStatusbar)));
        }

        /// <summary>
        /// Starts status bar animation.
        /// </summary>
        public void StartStatusBarAnimation(string message)
        {
            DoAnimation(AnimationState.Start, message);
        }

        /// <summary>
        /// Stops status bar animation.
        /// </summary>
        public void StopStatusBarAnimation()
        {
            DoAnimation(AnimationState.Stop, string.Empty);
        }

        #region Private methods

        private void DoAnimation(AnimationState state, string message)
        {
            try
            {
                _statusBar.Value.SetText(message);
                _statusBar.Value.Animation((int)state, ref StatusBarIconID);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning(ex.Message, ex);
            }
        }

        enum AnimationState
        {
            Stop = 0,
            Start = 1
        }

        #endregion
    }
}