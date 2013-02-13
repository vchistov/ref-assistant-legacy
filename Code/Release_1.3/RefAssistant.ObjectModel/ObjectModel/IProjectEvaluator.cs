//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com),
//         Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;

using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// Evaluate and prepare project information.
    /// </summary>
    interface IProjectEvaluator
    {
        #region Properties

        /// <summary>
        /// Get project information.
        /// </summary>
        ProjectInfo ProjectInfo { get; }

        /// <summary>
        /// Get project assembly.
        /// </summary>
        AssemblyDefinition ProjectAssembly { get; }

        /// <summary>
        /// Get manifest assemblies.
        /// </summary>
        IEnumerable<AssemblyDefinition> ManifestAssemblies { get; }

        /// <summary>
        /// Get project assembly types.
        /// </summary>       
        IEnumerable<TypeDefinition> ProjectTypesDefinitions { get; }

        /// <summary>
        /// Get project assembly imported types.
        /// </summary>       
        IEnumerable<TypeDefinition> ProjectImportedTypesDefinitions { get; }

        /// <summary>
        /// Get project assembly type references.
        /// </summary>       
        IEnumerable<TypeDefinition> ProjectTypesReferences { get; }

        /// <summary>
        /// Get member references.
        /// </summary>
        IEnumerable<MemberReference> MemberReferences { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets candidates.
        /// </summary>
        /// <returns>Candidates to delete.</returns>
        IList<ProjectReference> GetCandidates();

        #endregion
    }
}