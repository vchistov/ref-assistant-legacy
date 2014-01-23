using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.ReflectionServices;

namespace Lardite.RefAssistant.Model.Processing.Data
{
    internal sealed class Project : IProject
    {
        internal Project(IVsProject project, IServiceConfigurator serviceConfigurator)
        {
            Contract.Requires(project != null);
            Contract.Requires(serviceConfigurator != null);

            this.Name = project.Name;
            this.Kind = project.Kind;
        }

        public VsProjectKinds Kind { get; private set; }

        public string Name { get; private set; }

        public IAssembly Assembly
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IProjectReference> ProjectRefs
        {
            get { throw new NotImplementedException(); }
        }

        public bool Equals(IProject other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(this.Name, other.Name, StringComparison.Ordinal);
        }
    }
}
