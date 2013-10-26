using System;
using System.Collections.Generic;
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

        protected VsBaseProject(Project project, Func<VSProject, VsReferenceController> refControllerFactory)
        {
            ThrowUtils.ArgumentNull(() => project);
            ThrowUtils.ArgumentNull(() => refControllerFactory);

            Project = project;
            _refController = new Lazy<VsReferenceController>(() => refControllerFactory((VSProject)Project.Object));
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

        public virtual Guid Kind
        {
            get
            {
                Guid kind;
                Guid.TryParse(Project.Kind, out kind);
                return kind;
            }
        }

        #region Helpers

        protected DTE DTE
        {
            get { return Project.DTE; }
        }

        #endregion
    }
}
