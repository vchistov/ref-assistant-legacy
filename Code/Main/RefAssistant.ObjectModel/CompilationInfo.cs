//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Provides the information on compilation.
    /// </summary>
    public class CompilationInfo
    {
        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="CompilationInfo"/> class.
        /// </summary>
        /// <param name="isSuccessed"></param>
        /// <param name="isClrAssembly"></param>
        public CompilationInfo(bool isSuccessed, bool isClrAssembly)
        {
            IsSuccessed = isSuccessed;
            IsClrAssembly = isClrAssembly;
        }

        #endregion // .ctor

        #region Properties

        /// <summary>
        /// The compilation is successful.
        /// </summary>
        public bool IsSuccessed { get; private set; }

        /// <summary>
        /// Indicates whether this is the CLR assembly or not.
        /// </summary>
        public bool IsClrAssembly { get; private set; }

        #endregion // Properties
    }
}
