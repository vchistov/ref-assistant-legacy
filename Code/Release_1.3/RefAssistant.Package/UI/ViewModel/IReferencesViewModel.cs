//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

namespace Lardite.RefAssistant.UI.ViewModel
{
    internal interface IReferencesViewModel
    {
        /// <summary>
        /// Update the list of project references according to user input, 
        /// i.e. need to exclude from the list references which user didn't select.
        /// </summary>
        void UpdateReferences();
    }
}
