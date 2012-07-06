//
// Copyright © 2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Lardite.RefAssistant.VsProxy.Commands
{
    internal abstract class CleanupCommandBase : OleMenuCommand
    {
        #region Fields

        private DTE _dte;
        private readonly StatusBar _statusBar;

        #endregion // Fields

        #region .ctor

        protected CleanupCommandBase(IServiceProvider serviceProvider, CommandID commandID, IShellGateway shellGateway)
            : base(OnExecute, null, OnBeforeQueryStatus, commandID)
        {
            this.ServiceProvider = serviceProvider;
            this.ShellGateway = shellGateway;

            _statusBar = new StatusBar(serviceProvider);            
        }

        #endregion // .ctor

        #region Properties

        /// <summary>
        /// Get the Service Provider instance.
        /// </summary>
        protected IServiceProvider ServiceProvider 
        {
            get; private set;
        }

        /// <summary>
        /// Get the Shell Gateway instance.
        /// </summary>
        protected IShellGateway ShellGateway
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the <see cref="DTE"/> instance.
        /// </summary>
        protected DTE DTE
        {
            get
            {
                return _dte ?? (_dte = this.ServiceProvider.GetService(typeof(DTE)) as DTE);
            }
        }

        #endregion // Protected methods

        #region Private/Protected methods

        /// <summary>
        /// A command executor.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private static void OnExecute(object sender, EventArgs e)
        {
            var self = sender as CleanupCommandBase;

            try
            {
                self._statusBar.StartStatusBarAnimation();
                self.OnExecute();
            }
            catch (Exception ex)
            {
#if DEBUG
                LogManager.OutputLog.Error(Resources.RemoveUnusedReferencesCmd_ErrorOccured, ex);
#else
                LogManager.OutputLog.Information(Resources.RemoveUnusedReferencesCmd_EndProcessFailed);
#endif
                LogManager.ErrorListLog.Error(Resources.RemoveUnusedReferencesCmd_ErrorOccured);
                LogManager.ActivityLog.Error(Resources.RemoveUnusedReferencesCmd_ErrorOccured, ex);
            }
            finally
            {
                if (self != null)
                {
                    self._statusBar.StopStatusBarAnimation();
                    self._statusBar.SetStatusBarText(string.Empty);
                }
            }
        }

        /// <summary>
        /// A handler of the BeforeQueryStatus event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private static void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var self = sender as CleanupCommandBase;
            self.Enabled = self.Visible = self.Supported = self.CanExecute(self);
        }

        /// <summary>
        /// Removing progress changed.
        /// </summary>
        protected void OnProgressChanged(object sender, ProgressEventArgs e)
        {
            string text = e.Progress == 100 ? e.Text : string.Format("[{1}%] {0}", e.Text, e.Progress.ToString());
            _statusBar.SetStatusBarText(text);
        }

        #endregion // Private methods

        #region Virtual methods

        /// <summary>
        /// Executes a command.
        /// </summary>
        protected virtual void OnExecute()
        {
        }

        /// <summary>
        /// Determines whether this instance can execute the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>true if this instance can execute the specified command; otherwise, false.</returns>
        protected virtual bool CanExecute(OleMenuCommand command)
        {
            return command != null;
        }

        #endregion // Virtual methods
    }
}
