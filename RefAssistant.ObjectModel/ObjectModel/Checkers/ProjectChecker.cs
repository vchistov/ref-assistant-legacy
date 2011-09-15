//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Check project assembly types.
    /// </summary>
    abstract class ProjectChecker : IProjectChecker, IDisposable
    {
        #region Fields

        protected IProjectEvaluator _evaluator;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of a <see cref="ProjectChecker"/> class.
        /// </summary>
        /// <param name="evaluator">The project evaluator.</param>
        protected ProjectChecker(IProjectEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        #endregion // .ctor

        #region IDisposable implementation

        /// <summary>
        /// Dispose object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _evaluator != null)
            {
                _evaluator = null;
            }
        }

        #endregion

        #region IProjectChecker implementation

        /// <summary>
        /// Checks project references in order to find unused.
        /// </summary>
        /// <returns>Returns list of unused references.</returns>
        public IEnumerable<ProjectReference> Check()
        {
            var checkers = from checker in GetTypeCheckers()
                           orderby checker.OrderNumber ascending
                           select checker;

            using (var checkerData = new CheckerSharedData(_evaluator.GetCandidates()))
            {
                foreach (var checker in checkers)
                {
                    checker.Check(checkerData, _evaluator);

                    if (!checkerData.HasCandidateReferences)
                        break;
                }
                return checkerData.CandidateReferences.ToList();
            }
        }

        #endregion // IProjectChecker implementation

        #region Protected methods

        /// <summary>
        /// Gets checkers collection.
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<ITypesChecker> GetTypeCheckers();

        #endregion // Protected methods
    }
}