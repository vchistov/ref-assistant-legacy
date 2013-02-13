//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.Linq;

using Lardite.RefAssistant.Extensions;
using Lardite.RefAssistant.ObjectModel.Checkers.Helpers;

using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Check assembly's imported types.
    /// </summary>
    sealed class ImportedTypesChecker : ITypesChecker
    {
        #region Fields

        private Dictionary<ProjectReference, AssemblyDefinition> _assemblyDefinitions;
        private ImportedTypeCheckHelper _helper;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of a <see cref="ImportedTypesChecker"/> class.
        /// </summary>
        public ImportedTypesChecker()
        {
            _assemblyDefinitions = new Dictionary<ProjectReference, AssemblyDefinition>();
            _helper = new ImportedTypeCheckHelper();
        }

        #endregion // .ctor

        #region ITypeChecker implementation

        /// <summary>
        /// Performs assembly's types checking.
        /// </summary>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        /// <param name="evaluator">The project evaluator.</param>
        public void Check(CheckerSharedData sharedData, IProjectEvaluator evaluator)
        {
            if (sharedData == null)
                throw Error.ArgumentNull("sharedData");

            if (evaluator == null)
                throw Error.ArgumentNull("evaluator");

            _assemblyDefinitions.Clear();

            CheckImportedClasses(sharedData, evaluator);

            if (sharedData.HasCandidateReferences)
            {
                CheckImportedInterfaces(sharedData, evaluator);
            }
        }

        /// <summary>
        /// Get or set order number of checker (if several checkers exist).
        /// </summary>
        public int OrderNumber { get; set; }

        #endregion // ITypeChecker implementation

        #region Private methods

        /// <summary>
        /// Check imported classes.
        /// </summary>
        /// <param name="checkerData">Used types and assemblies candidates.</param>
        /// <param name="evaluator">The project evaluator.</param>
        private void CheckImportedClasses(CheckerSharedData checkerData, IProjectEvaluator evaluator)
        {
            foreach (TypeDefinition typeDef in evaluator.ProjectImportedTypesDefinitions)
            {
                try
                {
                    _helper.Check(typeDef, checkerData);
                }
                catch (Exception ex)
                {
                    throw Error.CheckType(typeDef.AssemblyQualifiedName(),
                        string.Format(Resources.CleanExecutor_CheckInteropTypeException, typeDef.AssemblyQualifiedName()), ex);
                }
            }
        }

        /// <summary>
        /// Check imported interfaces.
        /// </summary>
        /// <param name="checkerData">Used types and assemblies candidates.</param>
        /// <param name="evaluator">The project evaluator.</param>
        private void CheckImportedInterfaces(CheckerSharedData checkerData, IProjectEvaluator evaluator)
        {
            IEnumerable<TypeReference> typeReferences = evaluator.ProjectImportedTypesDefinitions
                .SelectMany(item => item.GetInterfaces());

            IMetadataScope forwardedFrom;
            foreach (TypeReference typeRef in typeReferences)
            {
                try
                {
                    TypeDefinition typeDef = typeRef.Resolve(out forwardedFrom);
                    checkerData.RemoveFromCandidates(forwardedFrom);
                    if (typeDef != null)
                    {
                        _helper.Check(typeDef, checkerData);
                    }

                    if (!checkerData.HasCandidateReferences)
                        break;
                }
                catch (Exception ex)
                {
                    throw Error.CheckType(typeRef.AssemblyQualifiedName(),
                        string.Format(Resources.CleanExecutor_CheckInteropTypeException, typeRef.AssemblyQualifiedName()), ex);
                }
            }
        }

        #endregion // Private methods
    }
}