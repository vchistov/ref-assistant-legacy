//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Default checker of assembly types.
    /// </summary>
    sealed class DefaultProjectChecker : ProjectChecker
    {
        #region Constants

        /// <summary>
        /// Project kind GUID.
        /// </summary>
        public const string ProjectTypeString = "00000000-0000-0000-0000-000000000000";

        #endregion // Constants

        #region .ctor

        /// <summary>
        /// Initialize a new instance of a <see cref="DefaultProjectChecker"/> class.
        /// </summary>
        /// <param name="evaluator">The project evaluator.</param>
        public DefaultProjectChecker(IProjectEvaluator evaluator)
            : base(evaluator)
        {
        }

        #endregion // .ctor

        #region Protected methods

        /// <summary>
        /// Gets checkers collection.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<ITypesChecker> GetTypeCheckers()
        {
            return new List<ITypesChecker>
                {
                    new AssemblyManifestTypesChecker() { OrderNumber = 0 },
                    new ClassHierarchyTypesChecker() { OrderNumber = 10 },
                    new InterfacesTypesChecker() { OrderNumber = 20 },
                    new AttributesTypesChecker() { OrderNumber = 40 },
                    new DependentAssembliesTypesChecker() { OrderNumber = 50 },
                    new ImportedTypesChecker() { OrderNumber = 60 }
                };
        }

        #endregion // Public methods
    }
}

