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
    internal class RemoveProjectReferencesCommand : CleanupCommandBase
    {
        #region Constants

        public const int ID = 0x100;

        #endregion // Constants

        #region .ctor

        public RemoveProjectReferencesCommand(IServiceProvider serviceProvider, IShellGateway shellGateway)
            : base(serviceProvider, new CommandID(typeof(RemoveProjectReferencesCommand).GUID, ID), shellGateway)
        {
        }

        #endregion // .ctor

        #region Overrides

        /// <summary>
        /// Executes the command.
        /// </summary>
        protected override void OnExecute()
        {
            var activeProjectGuid = Guid.Parse(DTEHelper.GetActiveProject(ServiceProvider).Kind);
            LogManager.ActivityLog.Information(string.Format(Resources.RemoveProjectReferencesCmd_StartRemoving, activeProjectGuid.ToString("D")));            

            using (var manager = new ExtensionManager(this.ShellGateway))
            {
                manager.ProgressChanged += OnProgressChanged;
                manager.StartProjectCleanup();
            }
        }

        protected override bool CanExecute(OleMenuCommand command)
        {
            return this.ShellGateway.CanRemoveUnusedReferences(null);
        }

        #endregion // Overrides        
    }
}
