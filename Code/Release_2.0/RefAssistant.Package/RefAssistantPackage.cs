﻿//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using Lardite.RefAssistant.UI;
using Lardite.RefAssistant.VsProxy.Commands;

using Microsoft.VisualStudio.Shell;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#1001", "#1004", "1.2.12190.4000", IconResourceID = 400, LanguageIndependentName = "References Assistant")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideProfile(typeof(GeneralOptionsPage), "References Assistant", "General", 1001, 1002, true, DescriptionResourceID = 1003)]
    [ProvideOptionPage(typeof(GeneralOptionsPage), "References Assistant", "General", 1001, 1002, true)]
#if VS10
    [Guid(GuidList.guidRefAssistant100PkgString)]    
#elif VS11
    [Guid(GuidList.guidRefAssistant110PkgString)]    
#endif    
    public sealed class RefAssistantPackage : Package
    {
        #region Overriden Package Implementation

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            log4net.Config.XmlConfigurator.Configure(new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "App.config")));

            LogManager.ActivityLog = new ActivityLog();
            LogManager.ErrorListLog = new ErrorListLog(this);
            LogManager.OutputLog = new OutputLog(this);

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                IExtensionOptions options = (IExtensionOptions)GetDialogPage(typeof(GeneralOptionsPage));
                var shellGateway = new ShellGateway(this, options);

                // Create the command for the menu item.
                mcs.AddCommand(new RemoveProjectReferencesCommand(this, shellGateway, options));
            }
        }

        #endregion // Overriden Package Implementation
    }
}