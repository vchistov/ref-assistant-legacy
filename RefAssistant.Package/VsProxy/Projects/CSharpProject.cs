using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Lardite.RefAssistant.Model.Projects;
using VSLangProj80;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    internal sealed class CSharpProject : IVsProjectExtended
    {
        private readonly Project _project;

        public CSharpProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            _project = project;
        }

        public string Name
        {
            get { return _project.Name; }
        }

        public string OutputAssemblyPath
        {
            get { return GetOutputAssemblyPath(); }
        }

        public IEnumerable<VsProjectReference> References
        {
            get { return GetProjectReferences(); }
        }

        public void RemoveReferences(IEnumerable<VsProjectReference> references)
        {
            IEnumerable<Reference3> projectReferences = ((VSProject2)_project.Object).References.Cast<Reference3>();
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

        public void RemoveAndSortUsings()
        {
            var alreadyOpenFiles = new HashSet<string>(
                _project.DTE.Documents.Cast<Document>().Select(d => d.FullName), 
                StringComparer.OrdinalIgnoreCase);

            var codeFiles = new ProjectItemIterator(_project.ProjectItems)
                .Where(item => item.FileCodeModel != null);

            foreach (ProjectItem file in codeFiles)
            {
                string fileName = file.get_FileNames(0);

                Window window = _project.DTE.OpenFile(EnvDTE.Constants.vsViewKindTextView, fileName);
                window.Activate();

                try
                {
                    _project.DTE.ExecuteCommand("Edit.RemoveAndSort", string.Empty);
                }
                catch (COMException e)
                {
                    //Do nothing, go to the next item
                    if (LogManager.ActivityLog != null)
                        LogManager.ActivityLog.Error(null, e);
                }

                if (alreadyOpenFiles.Contains(fileName))
                {
                    _project.DTE.ActiveDocument.Save();
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
            string outputPath = _project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
            string buildPath = _project.Properties.Item("LocalPath").Value.ToString();
            string targetName = _project.Properties.Item("OutputFileName").Value.ToString();

            return Path.Combine(buildPath, outputPath, targetName);
        }

        private IEnumerable<VsProjectReference> GetProjectReferences()
        {
            var proj = (VSProject2)_project.Object;
            var projectReferences = ((VSProject2)_project.Object).References;

            foreach (Reference3 projectRef in projectReferences)
            {
                yield return new VsProjectReference(
                    projectRef.Name, 
                    projectRef.Path, 
                    Version.Parse(projectRef.Version), 
                    string.Equals(projectRef.Culture, "0", StringComparison.Ordinal) ? string.Empty : projectRef.Culture,
                    projectRef.SpecificVersion);
            }
        }

        #endregion
    }
}
