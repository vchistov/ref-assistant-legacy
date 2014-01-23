using System;
using System.Diagnostics.Contracts;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.ReflectionServices;

namespace Lardite.RefAssistant.Model.Processing.Data
{
    internal sealed class ProjectReference : IProjectReference, IEquatable<ProjectReference>
    {
        private readonly Lazy<IAssembly> _assembly;

        internal ProjectReference(
            IAssemblyService assemblyService,
            ITypeService typeService,
            ICustomAttributeService customAttributeService,
            VsProjectReference projectReference)
        {
            Contract.Requires(assemblyService != null);
            Contract.Requires(typeService != null);
            Contract.Requires(customAttributeService != null);
            Contract.Requires(projectReference != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(projectReference.Location));

            this.Name = projectReference.Name;

            _assembly = new Lazy<IAssembly>(() => new Assembly(
                assemblyService, 
                typeService, 
                customAttributeService, 
                projectReference.Location));
        }

        public string Name { get; private set; }

        public IAssembly Assembly
        {
            get { return _assembly.Value; }
        }

        #region Object overrides

        public bool Equals(ProjectReference other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(this.Name, other.Name, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProjectReference);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }
}
