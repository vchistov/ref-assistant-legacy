//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Lardite.RefAssistant.Extensions;

using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// Contains project info.
    /// </summary>
    [Serializable]
    sealed class ProjectEvaluator : IProjectEvaluator
    {
        #region Fields

        private const string MsCorLib = "mscorlib";
        private const string SystemCore = "System.Core";

        [NonSerializedAttribute]
        private AssemblyDefinition _projectAssembly;
        private IEnumerable<AssemblyDefinition> _manifestAssemblies;
        private IEnumerable<TypeDefinition> _projectTypesDefinitions;
        private IEnumerable<TypeDefinition> _projectImportedTypesDefinitions;
        private List<TypeDefinition> _projectTypesReferences = null;
        private IEnumerable<MemberReference> _memberReferences;
        private IAssemblyResolver _assemblyResolver;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of a <see cref="ProjectEvaluator"/> class.
        /// </summary>
        /// <param name="projectInfo">Information about analyzed project.</param>        
        public ProjectEvaluator(ProjectInfo projectInfo)
        {
            if (projectInfo == null)
            {
                throw new ArgumentNullException("projectInfo");
            }

            ProjectInfo = projectInfo;
        }

        #endregion // .ctor

        #region Properties

        /// <summary>
        /// Get project information.
        /// </summary>
        public ProjectInfo ProjectInfo { get; private set; }

        /// <summary>
        /// Project assembly.
        /// </summary>
        public AssemblyDefinition ProjectAssembly
        {
            get
            {
                if (_projectAssembly == null)
                {
                    if (File.Exists(ProjectInfo.AssemblyPath))
                    {
                        var parameters = new ReaderParameters(ReadingMode.Deferred) { AssemblyResolver = AssemblyResolver };
                        _projectAssembly = AssemblyDefinition.ReadAssembly(ProjectInfo.AssemblyPath, parameters);
                    }
                }
                return _projectAssembly;
            }
        }

        /// <summary>
        /// Manifest assemblies.
        /// </summary>
        public IEnumerable<AssemblyDefinition> ManifestAssemblies
        {
            get
            {
                if (_manifestAssemblies == null)
                {
                    _manifestAssemblies = ProjectAssembly == null
                        ? new List<AssemblyDefinition>()
                        : (from module in ProjectAssembly.Modules
                           from asmRef in module.AssemblyReferences
                           select AssemblyResolver.Resolve(asmRef));
                }
                return _manifestAssemblies;
            }
        }

        /// <summary>
        /// Project assembly types.
        /// </summary>       
        public IEnumerable<TypeDefinition> ProjectTypesDefinitions
        {
            get
            {
                if (_projectTypesDefinitions == null)
                {
                    _projectTypesDefinitions = (ProjectAssembly == null)
                        ? new List<TypeDefinition>()
                        : ProjectAssembly.Modules.GetTypesDefinitions()
                            .Where(item => ((item.BaseType != null && item.BaseType.Scope.MetadataScopeType == MetadataScopeType.AssemblyNameReference) || item.IsInterface == true));
                }
                return _projectTypesDefinitions;
            }
        }

        /// <summary>
        /// Project assembly imported types.
        /// </summary>       
        public IEnumerable<TypeDefinition> ProjectImportedTypesDefinitions
        {
            get
            {
                if (_projectImportedTypesDefinitions == null)
                {
                    _projectImportedTypesDefinitions = (ProjectAssembly == null)
                        ? new List<TypeDefinition>()
                        : ProjectAssembly.Modules.GetTypesDefinitions()
                            .Where(item => item.BaseType == null && item.IsImport);
                }
                return _projectImportedTypesDefinitions;
            }
        }

        /// <summary>
        /// Project assembly type references.
        /// </summary>       
        public IEnumerable<TypeDefinition> ProjectTypesReferences
        {
            get
            {
                if (_projectTypesReferences == null)
                {
                    _projectTypesReferences = new List<TypeDefinition>();

                    if (ProjectAssembly == null)
                        return _projectTypesReferences;

                    var typeReferences = ProjectAssembly.Modules
                        .SelectMany(module => module.GetTypeReferences());

                    IMetadataScope forwardedFrom;
                    foreach (TypeReference typeReference in typeReferences)
                    {
                        var typeDef = typeReference.Resolve(out forwardedFrom);
                        if (typeDef != null)
                        {
                            _projectTypesReferences.Add(typeDef);
                        }
                    }
                }
                return _projectTypesReferences;
            }
        }

        /// <summary>
        /// Member references.
        /// </summary>
        public IEnumerable<MemberReference> MemberReferences
        {
            get
            {
                if (_memberReferences == null)
                {
                    _memberReferences = ProjectAssembly == null
                        ? new List<MemberReference>()
                        : ProjectAssembly.Modules.SelectMany(item => item.GetMemberReferences());
                }
                return _memberReferences;
            }
        }

        /// <summary>
        /// Assembly resolver.
        /// </summary>
        private IAssemblyResolver AssemblyResolver
        {
            get
            {
                return _assemblyResolver ?? (_assemblyResolver = InitAssemblyResolver());
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Get candidates.
        /// </summary>
        /// <returns>Candidates to delete.</returns>
        public IList<ProjectReference> GetCandidates()
        {
            var candidates = new List<ProjectReference>();
            foreach (ProjectReference projectReference in ProjectInfo.References)
            {
                if (projectReference.Name.Equals(SystemCore, StringComparison.OrdinalIgnoreCase)
                 || projectReference.Name.Equals(MsCorLib, StringComparison.OrdinalIgnoreCase))
                    continue;

                candidates.Add(projectReference);
            }
            return candidates;
        }

        /// <summary>
        /// Initialize assembly resolver.
        /// </summary>
        /// <returns>Assembly resolver.</returns>
        private IAssemblyResolver InitAssemblyResolver()
        {
            var assemblyResolver = new ProjectReferenceBasedAssemblyResolver(ProjectInfo.References);
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(ProjectInfo.AssemblyPath));
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            foreach (var refPath in GetProjectReferencesLocations())
            {
                assemblyResolver.AddSearchDirectory(refPath);
            }

            return assemblyResolver;
        }

        /// <summary>
        /// Gets list of locations when placed the referenced assemblies.
        /// </summary>
        /// <returns>Returns list of locations, exclude GAC locations.</returns>
        private IEnumerable<string> GetProjectReferencesLocations()
        {
            var gacRootFolder = Path.Combine(Environment.ExpandEnvironmentVariables("%systemroot%"), @"assembly\");
            
            return ProjectInfo.References
                .Select(r => Path.GetDirectoryName(r.Location).ToUpper())
                .Distinct()
                .Where(r => !r.StartsWith(gacRootFolder, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}
