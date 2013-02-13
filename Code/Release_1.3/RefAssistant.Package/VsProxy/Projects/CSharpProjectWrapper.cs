//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com),
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    /// <summary>
    /// Wraps C# project.
    /// </summary>
    internal class CSharpProjectWrapper : BaseProjectWrapper
    {
        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="CSharpProjectWrapper"/> class.
        /// </summary>
        /// <param name="vsProject">The Visual Studio project.</param>
        public CSharpProjectWrapper(Project vsProject)
            : base(vsProject)
        { }

        #endregion // .ctor

        #region Public methods

        /// <summary>
        /// Removes unused usings from project classes.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        public override void RemoveUnusedUsings(System.IServiceProvider serviceProvider)
        {
            base.RemoveUnusedUsings(serviceProvider);

            RunningDocumentTable docTable = new RunningDocumentTable(serviceProvider);
            var alreadyOpenFiles = docTable.Select(info => info.Moniker).ToList();

            string fileName;
            foreach (ProjectItem projectItem in new ProjectItemIterator(Project.ProjectItems).Where(item => item.FileCodeModel != null))
            {
                fileName = projectItem.get_FileNames(0);

                Window window = Project.DTE.OpenFile(EnvDTE.Constants.vsViewKindTextView, fileName);
                window.Activate();

                try
                {
                    Project.DTE.ExecuteCommand("Edit.RemoveAndSort", string.Empty);
                }
                catch (COMException e)
                {
                    //Do nothing, go to the next item
                    if (LogManager.ActivityLog != null)
                        LogManager.ActivityLog.Error(null, e);
                }

                if (alreadyOpenFiles.SingleOrDefault(file => file.Equals(fileName, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    Project.DTE.ActiveDocument.Save(fileName);
                }
                else
                {
                    window.Close(vsSaveChanges.vsSaveChangesYes);
                }
            }
        }

        #endregion // Public methods
    }
}
