//
// Copyright © 2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.ComponentModel.Design;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Lardite.RefAssistant.VsProxy.Commands
{
    [GuidAttribute(GuidList.guidRefAssistantCmdSetString)]
    internal sealed class RemoveProjectReferencesCommand : OleMenuCommand
    {
        private const int ID = 0x100;

        private readonly StatusBar _statusBar;
        private readonly IExtensionOptions _options;
        private readonly VsFacade _facade;

        public RemoveProjectReferencesCommand(IServiceProvider serviceProvider, IExtensionOptions options)
            : base(OnExecuteRemoving, null, OnBeforeQueryStatus, new CommandID(typeof(RemoveProjectReferencesCommand).GUID, ID))
        {
            ThrowUtils.ArgumentNull(() => serviceProvider);
            ThrowUtils.ArgumentNull(() => options);

            _options = options;
            _statusBar = new StatusBar(serviceProvider);
            _facade = new VsFacade(serviceProvider);
        }

        private static void OnExecuteRemoving(object sender, EventArgs e)
        {
            RemoveProjectReferencesCommand self = EnsureCommand(sender);
            try
            {
                self._statusBar.StartStatusBarAnimation(Resources.RemoveProjectReferencesCommand_Searching);

                var manager = new ExtensionManager(self._options, self._facade);
                manager.ProcessProject(self._facade.GetActiveProject());
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(Resources.RemoveProjectReferencesCommand_ErrorOccured, ex);
            }
            finally
            {
                if (self != null)
                {
                    self._statusBar.StopStatusBarAnimation();
                }
            }
        }

        private static void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            RemoveProjectReferencesCommand self = EnsureCommand(sender);

            self.Enabled = self.Visible = self.CanExecuteRemoving();
        }

        private bool CanExecuteRemoving()
        {
            try
            {
#warning TODO: replace by more 'transparent' code
                return !_facade.IsBuildInProgress() 
                    && (_facade.GetActiveProject() != null);
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }               

        private static RemoveProjectReferencesCommand EnsureCommand(object cmd)
        {
            Contract.Requires(cmd is RemoveProjectReferencesCommand);

            return (cmd as RemoveProjectReferencesCommand);
        }
    }
}