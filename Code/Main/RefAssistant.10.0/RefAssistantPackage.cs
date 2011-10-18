//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Lardite.RefAssistant.UI;
using Lardite.RefAssistant.Utils;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#1001", "#1004", "1.1.11290.2500", IconResourceID = 400, LanguageIndependentName = "References Assistant")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideProfile(typeof(GeneralOptionsPage), "References Assistant", "General", 1001, 1002, true, DescriptionResourceID = 1003)]
    [ProvideOptionPage(typeof(GeneralOptionsPage), "References Assistant", "General", 1001, 1002, true)]
    [Guid(GuidList.guidRefAssistantPkgString)]
    [ProvideLoadKey("Standard", "10.0", "References Assistant", "Lardite Group", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class RefAssistantPackage : Package
    {
        #region Constants

        private object statusBarIcon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_General;

        #endregion

        #region Fields

        private IShellGateway _shellGateway;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Default constructor of the package.
        /// </summary>
        public RefAssistantPackage()
        {            
        }

        #endregion // .ctor

        #region Overriden Package Implementation

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            LogManager.ActivityLog = new ActivityLog(this);
            LogManager.ErrorListLog = new ErrorListLog(this);
            LogManager.OutputLog = new OutputLog(this);

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuRemoveUnusedReferencesCommand = new CommandID(
                    GuidList.guidRefAssistantCmdSet, (int)PkgCmdIDList.cmdidRemoveUnusedReferencesCommand);
                OleMenuCommand menuItem = new OleMenuCommand(RemoveUnusedReferencesCommand_Exec,
                    menuRemoveUnusedReferencesCommand);
                menuItem.BeforeQueryStatus += RemoveUnusedReferencesCommand_BeforeQueryStatus;
                mcs.AddCommand(menuItem);
            }
        }

        #endregion // Overriden Package Implementation

        #region Events handlers

        /// <summary>
        /// Checking up of possibility to show command in the menu.
        /// </summary>
        /// <param name="sender">OleMenuCommand.</param>
        /// <param name="e">Arguments of the command.</param>
        private void RemoveUnusedReferencesCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            var oleMenuCmd = sender as OleMenuCommand;
            if (oleMenuCmd != null)
            {
                oleMenuCmd.Visible = ShellGateway.CanRemoveUnusedReferences(null);
            }
        }        

        /// <summary>
        /// Execute Remove Unused Reference command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveUnusedReferencesCommand_Exec(object sender, EventArgs e)
        {
            var oleMenuCmd = sender as OleMenuCommand;
            if (oleMenuCmd != null && 
                oleMenuCmd.CommandID.ID == (int)PkgCmdIDList.cmdidRemoveUnusedReferencesCommand && 
                ShellGateway.CanRemoveUnusedReferences(null))
            {
                try
                {
                    var activeProjectGuid = Guid.Parse(DTEHelper.GetActiveProject(this).Kind);

                    LogManager.ActivityLog.Information(string.Format(Resources.RefAssistantPackage_StartRemoving, activeProjectGuid.ToString("D")));

                    StartStatusBarAnimation();

                    using (var manager = new ExtensionManager(ShellGateway))
                    {
                        manager.ProgressChanged += manager_ProgressChanged;
                        manager.StartCleanup();
                    }
                }
                catch (Exception ex)
                {                    
#if DEBUG
                    LogManager.OutputLog.Error(Resources.RefAssistantPackage_ErrorOccured, ex);
#else
                    LogManager.OutputLog.Information(Resources.RefAssistantPackage_EndProcessFailed);
#endif
                    LogManager.ErrorListLog.Error(Resources.RefAssistantPackage_ErrorOccured);
                    LogManager.ActivityLog.Error(Resources.RefAssistantPackage_ErrorOccured, ex);
                }
                finally
                {
                    StopStatusBarAnimation();
                    SetStatusBarText(string.Empty);
                }
            }
        }

        /// <summary>
        /// Removing progress changed.
        /// </summary>
        private void manager_ProgressChanged(object sender, ProgressEventArgs e)
        {
            string text = e.Progress == 100 ? e.Text : string.Format("[{1}%] {0}", e.Text, e.Progress.ToString());
            SetStatusBarText(text);            
        } 

        #endregion // Events handlers

        #region Private methods/properties

        /// <summary>
        /// Status bar.
        /// </summary>
        private IVsStatusbar Statusbar
        {
            get { return (IVsStatusbar)GetService(typeof(SVsStatusbar)); }
        }

        /// <summary>
        /// Shell gateway.
        /// </summary>
        private IShellGateway ShellGateway
        {
            get
            {
                return _shellGateway ?? (_shellGateway = new ShellGateway(this, GetDialogPage(typeof(GeneralOptionsPage)) as IExtensionOptions));
            }
        }

        #region Status bar

        /// <summary>
        /// Starts status bar animation.
        /// </summary>
        private void StartStatusBarAnimation()
        {
            try
            {
                Statusbar.SetText(string.Empty);
                Statusbar.Animation(1, ref statusBarIcon);
            }
            catch (Exception ex)
            {
                LogManager.ErrorListLog.Error(string.Empty, ex);
            }
        }

        /// <summary>
        /// Stops status bar animation.
        /// </summary>
        private void StopStatusBarAnimation()
        {
            try
            {
                Statusbar.Animation(0, ref statusBarIcon);
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
        private void SetStatusBarText(string text)
        {
            try
            {
                Statusbar.SetText(text);
            }
            catch (Exception ex)
            {
                LogManager.ErrorListLog.Error(string.Empty, ex);
            }
        }

        #endregion

        #endregion // Private methods/properties
    }
}
