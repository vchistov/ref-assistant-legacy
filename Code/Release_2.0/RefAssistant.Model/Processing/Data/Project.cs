﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.ReflectionServices;

namespace Lardite.RefAssistant.Model.Processing.Data
{
    internal sealed class Project : IProject, IEquatable<Project>
    {
        private readonly Lazy<IAssembly> _assembly;
        private readonly Lazy<IList<IProjectReference>> _projectRefs;

        internal Project(
            IAssemblyService assemblyService,
            ITypeService typeService,
            ICustomAttributeService customAttributeService,
            IVsProject project)
        {
            Contract.Requires(assemblyService != null);
            Contract.Requires(typeService != null);
            Contract.Requires(customAttributeService != null);
            Contract.Requires(project != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(project.OutputAssemblyPath));

            _assembly = new Lazy<IAssembly>(() => new Assembly(
                assemblyService, 
                typeService, 
                customAttributeService, 
                project.OutputAssemblyPath));

            _projectRefs = new Lazy<IList<IProjectReference>>(() => LoadProjectReferences(
                assemblyService,
                typeService,
                customAttributeService,
                project));

            this.Name = project.Name;
            this.Kind = project.Kind;
        }

        public VsProjectKinds Kind { get; private set; }

        public string Name { get; private set; }

        public IAssembly Assembly
        {
            get { return _assembly.Value; }
        }

        public IEnumerable<IProjectReference> ProjectRefs
        {
            get { return _projectRefs.Value; }
        }

        #region Object overrides

        public bool Equals(Project other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(this.Name, other.Name, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Project);
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

        #region Helpers

        private IList<IProjectReference> LoadProjectReferences(
            IAssemblyService assemblyService,
            ITypeService typeService,
            ICustomAttributeService customAttributeService,
            IVsProject project)
        {
            return project
                .References
                .Select(@ref => new ProjectReference(assemblyService, typeService, customAttributeService, @ref))
                .Cast<IProjectReference>()
                .ToList();
        }

        #endregion
    }
}
