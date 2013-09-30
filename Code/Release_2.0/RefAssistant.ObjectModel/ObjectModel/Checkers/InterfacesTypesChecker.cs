//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using Mono.Cecil;
using Lardite.RefAssistant.ObjectModel.Checkers.Helpers;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Assembly's interfaces checker.
    /// </summary>
    sealed class InterfacesTypesChecker : ITypesChecker
    {
        #region Fields

        private InterfaceCheckHelper _helper = new InterfaceCheckHelper();

        #endregion // Fields

        #region Public methods

        /// <summary>
        /// Performs assembly's types checking.
        /// </summary>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        /// <param name="evaluator">The project evaluator.</param>
        public void Check(CheckerSharedData sharedData, IProjectEvaluator evaluator)
        {
            foreach (TypeDefinition type in evaluator.ProjectTypesDefinitions)
            {
                _helper.Check(type, sharedData);
                if (!sharedData.HasCandidateReferences)
                    return;
            }
        }

        /// <summary>
        /// Get or set order number of checker (if several checkers exist).
        /// </summary>
        public int OrderNumber { get; set; }

        #endregion // Public methods
    }
}