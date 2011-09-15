//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using Lardite.RefAssistant.ObjectModel.Checkers.Helpers;

using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Type's attributes checker.
    /// </summary>
    sealed class AttributesTypesChecker : ITypesChecker
    {
        #region Fields

        private AttributeTypeCheckHelper _helper = new AttributeTypeCheckHelper();

        #endregion // Fields

        #region Public methods

        /// <summary>
        /// Performs checking of attributes of types of assembly.
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
