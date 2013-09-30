//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Check C#(csharp) project assembly types.
    /// </summary>
    sealed class CSharpProjectChecker : ProjectChecker
    {
        #region Constants

        /// <summary>
        /// Project kind GUID.
        /// </summary>
        public const string ProjectTypeString = "fae04ec0-301f-11d3-bf4b-00c04f79efbc";

        #endregion // Constants

        #region .ctor

        /// <summary>
        /// Initialize a new instance of a <see cref="CSharpProjectChecker"/> class.
        /// </summary>
        /// <param name="evaluator">The project evaluator.</param>
        public CSharpProjectChecker(IProjectEvaluator evaluator)
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
                    new ImportedTypesChecker() { OrderNumber = 60 },
                    new XamlTypesChecker() { OrderNumber = 70 }
                };
        }

        #endregion // Public methods       
    }
}
