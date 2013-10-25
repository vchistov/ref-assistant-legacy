﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Lardite.RefAssistant.Model.Projects;
using VSLangProj80;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    internal sealed class CSharpProject : VsBaseProject
    {
        public CSharpProject(Project project)
            : base(project)
        {
        }

        public override string OutputAssemblyPath
        {
            get { return GetOutputAssemblyPath(); }
        }

        public override IEnumerable<VsProjectReference> References
        {
            get { return GetProjectReferences(); }
        }

        public override void RemoveReferences(IEnumerable<VsProjectReference> references)
        {
            IEnumerable<Reference3> projectReferences = ((VSProject2)Project.Object).References.Cast<Reference3>();
            IEnumerable<Reference3> readyForRemoveRefs = projectReferences.Join(
                references, 
                pr => pr.Name, 
                r => r.Name, 
                (pr, r) => pr);
            
            foreach (var reference in readyForRemoveRefs)
            {
                reference.Remove();
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
                    if (LogManager.ActivityLog != null)
                        LogManager.ActivityLog.Error(null, e);
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

        #region Private methods

        private string GetOutputAssemblyPath()
        {
            string outputPath = Project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
            string buildPath = Project.Properties.Item("LocalPath").Value.ToString();
            string targetName = Project.Properties.Item("OutputFileName").Value.ToString();

            return Path.Combine(buildPath, outputPath, targetName);
        }

        private IEnumerable<VsProjectReference> GetProjectReferences()
        {
            var proj = (VSProject2)Project.Object;
            var projectReferences = ((VSProject2)Project.Object).References;

            foreach (Reference3 projectRef in projectReferences)
            {
                yield return BuildReference(
                    projectRef.Name,
                    projectRef.Path,
                    projectRef.Version,
                    projectRef.Culture,
                    projectRef.PublicKeyToken,
                    projectRef.SpecificVersion);
            }
        }

        #endregion
    }
}
