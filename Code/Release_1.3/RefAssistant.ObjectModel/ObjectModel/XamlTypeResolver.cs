//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Linq;

using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// Resolve type declared in XAML to <see cref="Mono.Cecil.TypeDefinition"/> class.
    /// </summary>
    class XamlTypeResolver : IXamlTypeResolver
    {
        #region Fields

        private readonly ProjectReferenceCache _cache;
        private readonly IProjectEvaluator _evaluator;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="XamlTypeResolver"/> class.
        /// </summary>
        /// <param name="evaluator">The project evaluator.</param>
        public XamlTypeResolver(IProjectEvaluator evaluator)
        {
            _evaluator = evaluator;
            _cache = new ProjectReferenceCache(_evaluator);
        }

        #endregion // .ctor

        #region IXamlTypeResolver implementation

        /// <summary>
        /// Resolve type.
        /// </summary>
        /// <param name="type">Type declaration.</param>
        /// <returns>Returns type definition of the specified type.</returns>
        public TypeDefinition Resolve(XamlTypeDeclaration type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            // if AssemblyName is specified
            AssemblyDefinition assemblyDef = null;
            if (!string.IsNullOrWhiteSpace(type.AssemblyName))
            {
                assemblyDef = GetAssemblyDefinition(type.AssemblyName);
                if (assemblyDef != null)
                {
                    return FindTypeDefinition(assemblyDef, type.Namespace, type.Name);
                }
            }

            // try find type by "xml namespace" and "name"
            foreach (var projectRef in _evaluator.ProjectInfo.References)
            {
                assemblyDef = _cache[projectRef];
                if (assemblyDef != null && assemblyDef.HasCustomAttributes)
                {
                    // gets XmlnsDefinition attributes of assembly
                    var xmlnsDefAttribs = assemblyDef.CustomAttributes
                        .Where(a => a.AttributeType.FullName.EndsWith(".XmlnsDefinitionAttribute")
                            && a.HasConstructorArguments
                            && a.ConstructorArguments.Count >= 2
                            && a.ConstructorArguments[0].Value.ToString().Equals(type.PreferredXamlNamespace, StringComparison.OrdinalIgnoreCase))
                        .Select(a => a.ConstructorArguments[1].Value.ToString());

                    foreach (var @namespace in xmlnsDefAttribs)
                    {
                        var typeDef = FindTypeDefinition(assemblyDef, @namespace, type.Name);
                        if (typeDef != null)
                        {
                            return typeDef;
                        }
                    }
                }
            }
            
            // found nothing
            return null;
        }

        #endregion // IXamlTypeResolver implementation

        #region Private methods

        private AssemblyDefinition GetAssemblyDefinition(string assemblyName)
        {
            // sometime AssemblyName field has full assembly name (name + version + culture + public key token)
            assemblyName = assemblyName.Split(',')[0];
            var projectReference = GetProjectReferenceByName(assemblyName);
            if (projectReference != null)
            {
                return _cache[projectReference];
            }

            // if required assembly is current assembly project.
            if (_evaluator.ProjectAssembly.Name.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
            {
                return _evaluator.ProjectAssembly;
            }

            return null;
        }

        private ProjectReference GetProjectReferenceByName(string assemblyName)
        {
            return _evaluator.ProjectInfo.References
                .Where(r => r.AssemblyName.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
        }

        private TypeDefinition FindTypeDefinition(AssemblyDefinition assemblyDef, string @namespace, string name)
        {
            if (assemblyDef != null)
            {
                var fullname = string.Format("{0}.{1}", @namespace, name.Replace("+", "/"));
                return assemblyDef.Modules
                    .Select(m => m.GetType(fullname))
                    .FirstOrDefault();
            }
            return null;
        }

        #endregion //Private methods

        #region Nested types

        /// <summary>
        /// Assembly definitions cache.
        /// </summary>
        private class ProjectReferenceCache : SimpleCache<ProjectReference, AssemblyDefinition>
        {
            #region .ctor

            private IProjectEvaluator _evaluator;
            public ProjectReferenceCache(IProjectEvaluator evaluator)
            {
                _evaluator = evaluator;
            }

            #endregion //.ctor

            #region SimpleCache overrides

            /// <summary>
            /// Gets assembly definition by <see cref="ProjectReference"/> object.
            /// </summary>
            /// <param name="key">The project reference.</param>
            /// <returns>Returns <see cref="AssemblyDefinition"/> object.</returns>
            protected override AssemblyDefinition GetValue(ProjectReference key)
            {
                var parameters = new ReaderParameters(ReadingMode.Deferred)
                    {
                        AssemblyResolver = _evaluator.ProjectAssembly.MainModule.AssemblyResolver
                    };
                return AssemblyDefinition.ReadAssembly(key.Location, parameters);
            }

            #endregion //SimpleCache overrides
        }

        #endregion // Nested types
    }
}
