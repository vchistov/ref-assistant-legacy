//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com),
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using Lardite.RefAssistant.ObjectModel.Checkers;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// The runner of unused references checker.
    /// </summary>
    [Serializable]
    sealed class CheckExecutor : MarshalByRefObject
    {
        #region Public methods

        /// <summary>
        /// Execute checking up.
        /// </summary>
        public IEnumerable<ProjectReference> Execute(IProjectEvaluator evaluator)
        {
            using (var checker = ProjectCheckerType.Create(evaluator))
            {
                return checker.Check();
            }
        }

        #endregion
    }
}