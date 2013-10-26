using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.Model.Projects;
using VSLangProj;

namespace Lardite.RefAssistant.VsProxy.Projects.References
{
    internal abstract class VsReferenceController
    {
        protected readonly VSProject Project;

        protected VsReferenceController(VSProject project)
        {
            ThrowUtils.ArgumentNull(() => project);

            Project = project;
        }

        public abstract IEnumerable<VsProjectReference> References { get; }

        public virtual void Remove(IEnumerable<VsProjectReference> references)
        {
            IEnumerable<Reference> projectRefs = Project.References.Cast<Reference>();
            IEnumerable<Reference> readyForRemoveRefs = projectRefs.Join(
                references,
                pr => pr.Name,
                r => r.Name,
                (pr, r) => pr);

            foreach (var reference in readyForRemoveRefs)
            {
                reference.Remove();
            }
        }

        #region Helpers

        protected VsProjectReference BuildReference(string name, string location, string version, string culture, string publicKeyToken, bool isSpecificVersion)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(location));
            Contract.Requires(!string.IsNullOrWhiteSpace(version));
            Contract.Requires(!string.IsNullOrWhiteSpace(culture));

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
