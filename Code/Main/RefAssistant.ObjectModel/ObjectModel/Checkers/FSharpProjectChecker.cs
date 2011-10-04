//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//

using System.Collections.Generic;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Check F#(fsharp) project assembly types.
    /// </summary>
    sealed class FSharpProjectChecker : ProjectChecker
    {
        #region Constants

        /// <summary>
        /// Project kind GUID.
        /// </summary>
        public const string ProjectTypeString = "f2a71f9b-5d33-465a-a702-920d77279786";

        #endregion // Constants

        #region .ctor

        /// <summary>
        /// Initialize a new instance of a <see cref="FSharpProjectChecker"/> class.
        /// </summary>
        /// <param name="evaluator">The project evaluator.</param>
        public FSharpProjectChecker(IProjectEvaluator evaluator)
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
                    new ClassHierarchyTypesChecker() { OrderNumber = 0 },
                    new InterfacesTypesChecker() { OrderNumber = 10 },
                    new AttributesTypesChecker() { OrderNumber = 20 },
                    new DependentAssembliesTypesChecker() { OrderNumber = 30 },
                    new ImportedTypesChecker() { OrderNumber = 40 }
                };
        }

        #endregion // Public methods       
    }
}