//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Check Visual C++/CLI project assembly types.
    /// </summary>
    sealed class VisualCppCliProjectChecker : ProjectChecker
    {
        #region Constants

        /// <summary>
        /// Project kind GUID.
        /// </summary>
        public const string ProjectTypeString = "8bc9ceb8-8b4a-11d0-8d11-00a0c91bc942";

        #endregion // Constants

        #region .ctor

        /// <summary>
        /// Initialize a new instance of a <see cref="VisualCppCliProjectChecker"/> class.
        /// </summary>
        /// <param name="evaluator">The project evaluator.</param>
        public VisualCppCliProjectChecker(IProjectEvaluator evaluator)
            : base(evaluator)
        {
        }

        #endregion // .ctor

        #region Protected methods

        protected override IEnumerable<ITypesChecker> GetTypeCheckers()
        {
            return new List<ITypesChecker>
                {
                    new ClassHierarchyTypesChecker() { OrderNumber = 0 },
                    new InterfacesTypesChecker() { OrderNumber = 10 },
                    new AttributesTypesChecker() { OrderNumber = 30 },
                    new DependentAssembliesTypesChecker() { OrderNumber = 40 },
                    new ImportedTypesChecker() { OrderNumber = 50 }
                };
        }

        #endregion // Protected methods
    }
}
