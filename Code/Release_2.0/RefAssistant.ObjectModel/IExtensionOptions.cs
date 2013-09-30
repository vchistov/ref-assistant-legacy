//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//         Chistov Victor (vchistov@lardite.com)
//

namespace Lardite.RefAssistant
{
    /// <summary>
    /// General settings of the extension.
    /// </summary>
    public  interface IExtensionOptions
    {
        /// <summary>
        /// Show the window containing list of removable references. Each of these references can be excluded from removable references.
        /// </summary>
        bool? IsShowUnusedReferencesWindow { get; set; }

        /// <summary>
        /// Apply the Remove Unused Using operation to all project files.
        /// </summary>
        bool? IsRemoveUsingsAfterRemoving { get; set; }
    }
}