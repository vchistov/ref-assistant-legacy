//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// Resolve assembly from FullName to <see cref="Mono.Cecil.AssemblyDefinition"/> by project's referenced assemblies.
    /// </summary>
    sealed class ProjectReferenceBasedAssemblyResolver : BaseAssemblyResolver
    {
        #region Fields

        private readonly IEnumerable<ProjectReference> _projectReferences;
        private readonly IDictionary<string, AssemblyDefinition> _cache;
        private readonly PublicKeyTokenConverter _converter;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="ProjectReferenceBasedAssemblyResolver"/> class.
        /// </summary>
        /// <param name="projectReferences">Project's references.</param>
        public ProjectReferenceBasedAssemblyResolver(IEnumerable<ProjectReference> projectReferences)
        {
            _projectReferences = projectReferences;
            _cache = new Dictionary<string, AssemblyDefinition>();
            _converter = new PublicKeyTokenConverter();
        }

        #endregion // .ctor

        #region Override

        /// <summary>
        /// Resolve assembly.
        /// </summary>
        /// <param name="name">Name of the assembly.</param>
        /// <returns>Returns resolved assembly.</returns>
        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            AssemblyDefinition assemblyDefinition = null;
            if (_cache.TryGetValue(name.FullName, out assemblyDefinition))
            {
                return assemblyDefinition;
            }

            var projectRef = _projectReferences.SingleOrDefault(item => IsEquals(name, item));
            assemblyDefinition = (projectRef != null && !projectRef.FullName.Equals(name.FullName, StringComparison.InvariantCultureIgnoreCase))
                ? ReadAssembly(projectRef) 
                : base.Resolve(name);

            return AddToCache(assemblyDefinition);
        }

        #endregion // Override

        #region Private methods

        /// <summary>
        /// Read assembly.
        /// </summary>
        /// <param name="projectReference">Project reference.</param>
        /// <returns>Assembly definition.</returns>
        private AssemblyDefinition ReadAssembly(ProjectReference projectReference)
        {
            var parameters = new ReaderParameters(ReadingMode.Deferred) { AssemblyResolver = this };
            return AssemblyDefinition.ReadAssembly(projectReference.Location, parameters);
        }

        /// <summary>
        /// Is equals.
        /// </summary>
        /// <param name="nameReference">Assembly name reference.</param>
        /// <param name="projectReference">Project reference.</param>
        /// <returns>If true, then equals.</returns>
        private bool IsEquals(AssemblyNameReference nameReference, ProjectReference projectReference)
        {
            bool result = string.Equals(nameReference.Name, projectReference.AssemblyName, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(nameReference.Culture, projectReference.Culture, StringComparison.InvariantCultureIgnoreCase);

            string token = (nameReference.PublicKeyToken == null || nameReference.PublicKeyToken.Length == 0)
                ? string.Empty
                : _converter.ConvertFrom(nameReference.PublicKeyToken);

            result &= string.Equals(token, projectReference.PublicKeyToken, StringComparison.InvariantCultureIgnoreCase);
            return result;
        }

        /// <summary>
        /// Add to cache.
        /// </summary>
        /// <param name="assemblyDefinition">Assembly definition.</param>
        /// <returns>Assembly definition.</returns>
        private AssemblyDefinition AddToCache(AssemblyDefinition assemblyDefinition)
        {
            if (!_cache.ContainsKey(assemblyDefinition.FullName))
            {
                _cache.Add(assemblyDefinition.FullName, assemblyDefinition);
            }

            return assemblyDefinition;
        }

        #endregion // Private methods
    }
}