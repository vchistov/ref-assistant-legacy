//
// Copyright © 2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Lardite.RefAssistant.VsProxy.Commands
{
    [GuidAttribute(GuidList.guidRefAssistantCmdSetString)]
    internal class RemoveSolutionReferencesCommand : CleanupCommandBase
    {
        #region Constants

        public const int ID = 0x110;

        #endregion // Constants

        #region .ctor

        public RemoveSolutionReferencesCommand(IServiceProvider serviceProvider, IShellGateway shellGateway)
            : base(serviceProvider, new CommandID(typeof(RemoveSolutionReferencesCommand).GUID, ID), shellGateway)
        {
        }

        #endregion // .ctor

        #region Overrides

        protected override void OnExecute()
        {
            LogManager.ActivityLog.Information(Resources.RemoveSolutionReferencesCmd_StartRemoving);

            using (var manager = new ExtensionManager(this.ShellGateway))
            {
                manager.ProgressChanged += OnProgressChanged;
                manager.StartSolutionCleanup();
            }
        }

        protected override bool CanExecute(OleMenuCommand command)
        {
            return !DTEHelper.IsBuildInProgress(DTE);
        }

        #endregion // Overrides
    }
}
