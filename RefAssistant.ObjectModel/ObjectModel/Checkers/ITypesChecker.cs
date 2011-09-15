//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Assembly's types checker interface.
    /// </summary>
    interface ITypesChecker
    {
        /// <summary>
        /// Performs assembly's types checking.
        /// </summary>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        /// <param name="evaluator">The project evaluator.</param>
        void Check(CheckerSharedData sharedData, IProjectEvaluator evaluator);

        /// <summary>
        /// Get or set order number of checker (if several checkers exist).
        /// </summary>
        int OrderNumber { get; set; }
    }
}
