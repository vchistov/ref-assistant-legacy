//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using Lardite.RefAssistant.ObjectModel.Checkers.Helpers;

using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Check assembly's types hierarchy.
    /// </summary>
    sealed class ClassHierarchyTypesChecker : ITypesChecker
    {
        #region Fields

        private ClassCheckHelper _classCheckHelper = new ClassCheckHelper();

        #endregion // Fields

        #region ITypeChecker implementation

        /// <summary>
        /// Performs assembly's types checking.
        /// </summary>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        /// <param name="evaluator">The project evaluator.</param>
        public void Check(CheckerSharedData sharedData, IProjectEvaluator evaluator)
        {
            foreach (TypeDefinition type in evaluator.ProjectTypesDefinitions)
            {
                _classCheckHelper.Check(type, sharedData);
                if (!sharedData.HasCandidateReferences)
                    return;
            }
        }

        /// <summary>
        /// Get or set order number of checker (if several checkers exist).
        /// </summary>
        public int OrderNumber { get; set; }

        #endregion // ITypeChecker implementation
    }
}
