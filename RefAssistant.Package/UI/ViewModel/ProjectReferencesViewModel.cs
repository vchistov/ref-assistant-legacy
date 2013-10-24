//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

using Lardite.RefAssistant.Model.Projects;

namespace Lardite.RefAssistant.UI.ViewModel
{
    internal sealed class ProjectReferencesViewModel : DependencyObject, IReferencesViewModel
    {
        private readonly IEnumerable<VsProjectReference> _references;

        /// <summary>
        /// Initialize a new instance of the <see cref="ProjectReferencesViewModel"/> class.
        /// </summary>
        /// <param name="references">The project's unused references.</param>
        public ProjectReferencesViewModel(IEnumerable<VsProjectReference> references)
        {
            ThrowUtils.ArgumentNull(() => references);

            _references = references;
            this.References = new ReadOnlyObservableCollection<ReferenceData>
                (
                    new ObservableCollection<ReferenceData>(references.Select(item => new ReferenceData(item)))
                );
        }

        #region Properties

        public ReadOnlyObservableCollection<ReferenceData> References
        {
            get;
            private set;
        }

        #endregion // Properties

        #region IReferencesViewModel implementation
       
        IEnumerable<VsProjectReference> IReferencesViewModel.SelectedReferences
        {
            get
            {
                return References.Where(@ref => @ref.IsUnused).Select(@ref => @ref.ProjectReference);
            }
        }

        #endregion // IReferencesViewModel implementation        
    }
}
