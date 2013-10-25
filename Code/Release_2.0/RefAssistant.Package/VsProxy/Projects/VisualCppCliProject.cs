using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Lardite.RefAssistant.Model.Projects;
using VSLangProj80;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    internal sealed class VisualCppCliProject : VsBaseProject
    {
        public VisualCppCliProject(Project project)
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

        #region Private methods

        private string GetOutputAssemblyPath()
        {
            var primaryOutput = Project.ConfigurationManager.ActiveConfiguration.OutputGroups.Item("Primary Output");
            if (primaryOutput != null && primaryOutput.FileCount > 0)
            {
                var url = ((object[])primaryOutput.FileURLs)[0].ToString();
                return new Uri(url).LocalPath;
            }
            return string.Empty;
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

        //private const string ManagedExtensions = "ManagedExtensions";
        //public bool IsManaged
        //{
        //    get
        //    {
        //        try
        //        {
        //            // me is Microsoft.VisualStudio.VCProject.compileAsManagedOptions enum
        //            var me = (int)Project.ConfigurationManager
        //                .ActiveConfiguration.Properties.Item(ManagedExtensions).Value;
        //            return me != 0; // not equals "managedNotSet"
        //        }
        //        catch
        //        {
        //            return false;
        //        }
        //    }
        //}

        #endregion
    }
}
