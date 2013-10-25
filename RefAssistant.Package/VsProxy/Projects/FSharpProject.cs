using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using Lardite.RefAssistant.Model.Projects;
using Microsoft.VisualStudio.FSharp.ProjectSystem.Automation;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    internal sealed class FSharpProject : VsBaseProject
    {
        public FSharpProject(Project project)
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
            IEnumerable<OAAssemblyReference> projectReferences = ((OAVSProject)Project.Object).References.Cast<OAAssemblyReference>();
            IEnumerable<OAAssemblyReference> readyForRemoveRefs = projectReferences.Join(
                references, 
                pr => pr.Name, 
                r => r.Name, 
                (pr, r) => pr);
            
            foreach (var reference in readyForRemoveRefs)
            {
                reference.Remove();
            }
        }

        #region Private methods

        private string GetOutputAssemblyPath()
        {
            string outputPath = Project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
            string fullPath = Project.Properties.Item("FullPath").Value.ToString();
            string targetName = Project.Properties.Item("OutputFileName").Value.ToString();

            return Path.Combine(fullPath, outputPath, targetName);
        }

        private IEnumerable<VsProjectReference> GetProjectReferences()
        {
            var projectReferences = ((OAVSProject)Project.Object).References;
            foreach (OAAssemblyReference projectRef in projectReferences)
            {
                yield return BuildReference(
                    projectRef.Name,
                    projectRef.Path,
                    projectRef.Version,
                    projectRef.Culture,
                    projectRef.PublicKeyToken,
                    false);
            }
        }

        #endregion
    }
}
