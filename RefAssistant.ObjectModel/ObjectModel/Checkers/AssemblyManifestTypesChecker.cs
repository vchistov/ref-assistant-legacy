//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Linq;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Check references by assembly manifest.
    /// </summary>
    sealed class AssemblyManifestTypesChecker : ITypesChecker
    {
        #region ITypeChecker implementation

        /// <summary>
        /// Performs assembly's types checking.
        /// </summary>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        /// <param name="evaluator">The project evaluator.</param>
        public void Check(CheckerSharedData sharedData, IProjectEvaluator evaluator)
        {
            var query = (from reference in sharedData.CandidateReferences
                        join assemblyRef in evaluator.ManifestAssemblies 
                            on reference.FullName.ToUpper() equals assemblyRef.FullName.ToUpper()
                        select reference).ToList();
            
            foreach (var reference in query)
            {
                sharedData.RemoveFromCandidates(reference);
            }
        }

        /// <summary>
        /// Get or set order number of checker (if several checkers exist).
        /// </summary>
        public int OrderNumber { get; set; }

        #endregion // ITypeChecker implementation
    }
}
