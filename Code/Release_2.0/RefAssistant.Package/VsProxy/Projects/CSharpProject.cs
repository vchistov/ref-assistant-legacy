using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE;
using Lardite.RefAssistant.Model.Projects;
using VSLangProj80;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    internal sealed class CSharpProject : IVsProject
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
