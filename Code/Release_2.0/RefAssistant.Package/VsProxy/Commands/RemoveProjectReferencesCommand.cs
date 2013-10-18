﻿//
// Copyright © 2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Lardite.RefAssistant.VsProxy.Commands
{
    [GuidAttribute(GuidList.guidRefAssistantCmdSetString)]
    internal sealed class RemoveProjectReferencesCommand : OleMenuCommand
    {
        public const int ID = 0x100;

        private DTE _dte;
        private readonly StatusBar _statusBar;
        private IServiceProvider _serviceProvider;
        private IShellGateway _shellGateway;

        public RemoveProjectReferencesCommand(IServiceProvider serviceProvider, IShellGateway shellGateway)
            : base(OnExecuteRemoving, null, OnBeforeQueryStatus, new CommandID(typeof(RemoveProjectReferencesCommand).GUID, ID))
        {
            _serviceProvider = serviceProvider;
            _shellGateway = shellGateway;
            _statusBar = new StatusBar(serviceProvider);
        }

        #region Methods

        private static void OnExecuteRemoving(object sender, EventArgs e)
        {
            var self = sender as RemoveProjectReferencesCommand;

            try
            {
                self._statusBar.StartStatusBarAnimation();
                self.OnExecuteRemoving();
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

        private static void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var self = sender as RemoveProjectReferencesCommand;

            self.Enabled = self.Visible = self.Supported = self.CanExecuteRemoving();
        }

        private bool CanExecuteRemoving()
        {
            return _shellGateway.CanRemoveUnusedReferences(null);
        }

        private void OnExecuteRemoving()
        {
            var activeProjectGuid = Guid.Parse(DTEHelper.GetActiveProject(_serviceProvider).Kind);
            LogManager.ActivityLog.Information(string.Format(Resources.RemoveProjectReferencesCmd_StartRemoving, activeProjectGuid.ToString("D")));

            using (var manager = new ExtensionManager(_shellGateway))
            {
                manager.ProgressChanged += OnRemovingProgressChanged;
                manager.StartProjectCleanup();
            }
        }                

        private void OnRemovingProgressChanged(object sender, ProgressEventArgs e)
        {
            string text = e.Progress == 100 ? e.Text : string.Format("[{1}%] {0}", e.Text, e.Progress.ToString());
            _statusBar.SetStatusBarText(text);
        }

        #endregion    
    }
}