using System;
using System.Collections.Generic;
using EnvDTE;
using Lardite.RefAssistant.Model.Projects;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    internal abstract class VsBaseProject : IVsProjectExtended
    {
        protected readonly Project Project;

        protected VsBaseProject(Project project)
        {
            ThrowUtils.ArgumentNull(() => project);

            Project = project;
        }

        public abstract string OutputAssemblyPath { get; }

        public abstract IEnumerable<VsProjectReference> References { get; }

        public abstract void RemoveReferences(IEnumerable<VsProjectReference> references);

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

        protected VsProjectReference BuildReference(string name, string location, string version, string culture, string publicKeyToken, bool isSpecificVersion)
        {
            return new VsProjectReference(
                    name,
                    location,
                    Version.Parse(version),
                    string.Equals(culture, "0", StringComparison.Ordinal) ? string.Empty : culture,
                    isSpecificVersion) { PublicKeyToken = publicKeyToken };
        }

        #endregion
    }
}
