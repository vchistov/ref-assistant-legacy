//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// The <see cref="ProjectChecker"/> factory.
    /// </summary>
    static class ProjectCheckerType
    {
        #region Public methods

        /// <summary>
        /// Create a new instance of a project checker class.
        /// </summary>
        /// <param name="evaluator">The project evaluator.</param>
        /// <returns>Returns created project checker.</returns>
        public static ProjectChecker Create(IProjectEvaluator evaluator)
        {
            switch (evaluator.ProjectInfo.Type.ToString("D"))
            {
                case CSharpProjectChecker.ProjectTypeString:
                    return new CSharpProjectChecker(evaluator);

                case VisualBasicProjectChecker.ProjectTypeString:
                    return new VisualBasicProjectChecker(evaluator);

                case VisualCppCliProjectChecker.ProjectTypeString:
                    return new VisualCppCliProjectChecker(evaluator);

                case FSharpProjectChecker.ProjectTypeString:
                    return new FSharpProjectChecker(evaluator);

                default:
                    return new DefaultProjectChecker(evaluator);
            }
        }

        #endregion // Public methods
    }
}
