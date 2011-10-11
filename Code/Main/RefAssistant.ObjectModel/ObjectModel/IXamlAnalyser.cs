//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// Analysing of XAML files.
    /// </summary>
    interface IXamlAnalyser
    {
        /// <summary>
        /// Gets types list which declared into XAML markup.
        /// </summary>
        /// <returns>Returns <see cref="Lardite.RefAssistant.ObjectModel.XamlTypeDeclaration"/> collection.</returns>
        IEnumerable<XamlTypeDeclaration> GetDeclaredTypes();
    }
}
