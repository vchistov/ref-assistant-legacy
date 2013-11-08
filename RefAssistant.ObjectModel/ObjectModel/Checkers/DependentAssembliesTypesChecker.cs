//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com),
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Linq;

using Mono.Cecil;

using Lardite.RefAssistant.Extensions;
using Lardite.RefAssistant.ObjectModel.Checkers.Helpers;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Check dependent assemblies.
    /// </summary>
    sealed class DependentAssembliesTypesChecker : ITypesChecker
    {
        #region Fields

        private ClassCheckHelper _classCheckHeper = new ClassCheckHelper();
        private InterfaceCheckHelper _interfaceCheckHelper = new InterfaceCheckHelper();
        private MemberReferencesCheckHelper _memberRefsCheckHelper = new MemberReferencesCheckHelper();

        #endregion // Fields

        #region ITypesChecker implementation

        /// <summary>
        /// Performs dependent assemblies checking.
        /// </summary>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        /// <param name="evaluator">The project evaluator.</param>
        public void Check(CheckerSharedData sharedData, IProjectEvaluator evaluator)
        {
            foreach (AssemblyDefinition manifestAssemblyDefinition in evaluator.ManifestAssemblies)
            {
                try
                {
                    CheckAssemblyTypes(manifestAssemblyDefinition, sharedData, evaluator);
                    if (!sharedData.HasCandidateReferences)
                        break;
                }
                catch (Exception ex)
                {
                    ex.Data.Add("AssemblyDefinition", manifestAssemblyDefinition.FullName);
                    throw;
                }
            }
        }

        /// <summary>
        /// Get or set order number of checker (if several checkers exist).
        /// </summary>
        public int OrderNumber { get; set; }

        #endregion // ITypesChecker implementation

        #region Private methods

        /// <summary>
        /// Check assembly types.
        /// </summary>
        /// <param name="assemblyDefinition">The assembly.</param>
        private void CheckAssemblyTypes(AssemblyDefinition assemblyDefinition, CheckerSharedData checkerData, IProjectEvaluator evaluator)
        {
            var referencedAssemblyTypes = evaluator.ProjectTypesReferences
                .Select(projectTypeRef => projectTypeRef)
                .Join(assemblyDefinition.Modules.GetTypesDefinitions().Where(moduleType => moduleType.BaseType != null),
                    projectTypeRef => projectTypeRef.FullName.ToLower(),
                    moduleType => moduleType.FullName.ToLower(),
                    (projectTypeRef, moduleType) => projectTypeRef);

            foreach (TypeDefinition referencedType in referencedAssemblyTypes)
            {
                if (checkerData.IsUsedTypeExists(referencedType.AssemblyQualifiedName()))
                {
                    _memberRefsCheckHelper.Check(referencedType, checkerData, evaluator);
                    if (!checkerData.HasCandidateReferences)
                        break;
                }
                else
                {
                    checkerData.RemoveFromCandidates(referencedType.Scope);
                    if (!checkerData.HasCandidateReferences)
                        break;

                    _classCheckHeper.Check(referencedType, checkerData);
                    if (!checkerData.HasCandidateReferences)
                        break;

                    _interfaceCheckHelper.Check(referencedType, checkerData);
                    if (!checkerData.HasCandidateReferences)
                        break;

                    _memberRefsCheckHelper.Check(referencedType, checkerData, evaluator);
                    if (!checkerData.HasCandidateReferences)
                        break;
                }
            }
        }

        #endregion // Private methods
    }
}
