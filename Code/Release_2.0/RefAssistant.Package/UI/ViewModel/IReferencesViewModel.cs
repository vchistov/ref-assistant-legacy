//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;
using Lardite.RefAssistant.Model.Contracts;

namespace Lardite.RefAssistant.UI.ViewModel
{
    internal interface IReferencesViewModel
    {
        IEnumerable<VsProjectReference> SelectedReferences { get; }
    }
}
