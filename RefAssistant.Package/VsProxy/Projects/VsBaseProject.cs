using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using EnvDTE;
using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.VsProxy.Projects.References;
using VSLangProj;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    internal abstract class VsBaseProject : IVsProjectExtended
    {
        protected readonly Project Project;
        private readonly Lazy<VsReferenceController> _refController;
        private readonly Lazy<ProjectKindAttribute> _projectKindAttribute;

        protected VsBaseProject(Project project, Func<VSProject, VsReferenceController> refControllerFactory)
        {
            ThrowUtils.ArgumentNull(() => project);
            ThrowUtils.ArgumentNull(() => refControllerFactory);

            Project = project;
            _refController = new Lazy<VsReferenceController>(() => refControllerFactory((VSProject)Project.Object));
            _projectKindAttribute = new Lazy<ProjectKindAttribute>(() => this.GetType().GetCustomAttribute<ProjectKindAttribute>());
        }

        public abstract string OutputAssemblyPath { get; }

        public virtual IEnumerable<VsProjectReference> References 
        {
            get { return _refController.Value.References; }                
        }

        public virtual void RemoveReferences(IEnumerable<VsProjectReference> references)
        {
            ThrowUtils.ArgumentNull(() => references);

            _refController.Value.Remove(references);
        }

        public virtual void RemoveAndSortUsings()
        {
            // Do nothing by default, since only C# project supports this operation.
        }

        public virtual string Name
        {
            get { return Project.Name; }
        }

        public virtual string Configuration
        {
            get
            {
                var activeConfiguration = Project.ConfigurationManager.ActiveConfiguration;
                return string.Format("{0} {1}",
                    activeConfiguration.ConfigurationName,
                    activeConfiguration.PlatformName);
            }
        }

        public VsProjectKinds Kind
        {
            get { return this.ProjectKindAttribute.Kind; }
        }

        public Guid KindGuid
        {
            get { return this.ProjectKindAttribute.Guid; }
        }

        #region Helpers

        protected DTE DTE
        {
            get { return Project.DTE; }
        }

        private ProjectKindAttribute ProjectKindAttribute
        {
            get
            {
                Contract.Ensures(Contract.Result<ProjectKindAttribute>() != null);
                return _projectKindAttribute.Value;
            }
        }

        #endregion
    }
}
