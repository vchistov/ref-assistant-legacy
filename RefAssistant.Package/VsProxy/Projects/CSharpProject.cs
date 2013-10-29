using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Lardite.RefAssistant.VsProxy.Projects.References;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    internal sealed class CSharpProject : VsBaseProject
    {
        public CSharpProject(Project project)
            : base(project, (vsp) => new Reference3Controller(vsp))
        {
        }

        public override string OutputAssemblyPath
        {
            get 
            {
                string outputPath = Project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
                string buildPath = Project.Properties.Item("LocalPath").Value.ToString();
                string targetName = Project.Properties.Item("OutputFileName").Value.ToString();

                return Path.Combine(buildPath, outputPath, targetName);
            }
        }

        public override void RemoveAndSortUsings()
        {
            var alreadyOpenFiles = new HashSet<string>(
                DTE.Documents.Cast<Document>().Select(d => d.FullName), 
                StringComparer.OrdinalIgnoreCase);

            var codeFiles = new ProjectItemIterator(Project.ProjectItems)
                .Where(item => item.FileCodeModel != null);

            foreach (ProjectItem file in codeFiles)
            {
                string fileName = file.get_FileNames(0);

                Window window = DTE.OpenFile(EnvDTE.Constants.vsViewKindTextView, fileName);
                window.Activate();

                try
                {
                    DTE.ExecuteCommand("Edit.RemoveAndSort", string.Empty);
                }
                catch (COMException e)
                {
                    //Do nothing, go to the next item
                    LogManager.Instance.Warning(Resources.CSharpProject_RemoveAndSortUsingsError, e);                    
                }

                if (alreadyOpenFiles.Contains(fileName))
                {
                    DTE.ActiveDocument.Save();
                }
                else
                {
                    window.Close(vsSaveChanges.vsSaveChangesYes);
                }
            }
        }
    }
}
