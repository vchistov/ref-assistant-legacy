//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

using Lardite.RefAssistant.ObjectModel;

namespace Lardite.RefAssistant.UI.ViewModel
{
    internal sealed class SolutionReferencesViewModel : DependencyObject, IReferencesViewModel
    {
        #region Fields

        private readonly Lazy<ReadOnlyObservableCollection<ProjectData>> _solutionReferences;
        private readonly IEnumerable<IProjectInspectResult> _solutionInspectResult;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="SolutionReferencesViewModel"/> class.
        /// </summary>
        /// <param name="unusedProjectRefs">The project's unused references.</param>
        public SolutionReferencesViewModel(IEnumerable<IProjectInspectResult> solutionInspectResult)
        {
            if (solutionInspectResult == null)
            {
                throw Error.ArgumentNull("solutionInspectResult");
            }

            _solutionInspectResult = solutionInspectResult;
            _solutionReferences = new Lazy<ReadOnlyObservableCollection<ProjectData>>
                (
                    () => new ReadOnlyObservableCollection<ProjectData>(GetSolutionReferencesList())
                );
        }

        #endregion // .ctor

        #region Properties

        /// <summary>
        /// Get list of the project references.
        /// </summary>
        public ReadOnlyObservableCollection<ProjectData> Projects
        {
            get
            {
                return _solutionReferences.Value;
            }
        }

        #endregion // Properties

        #region IReferencesViewModel implementation

        /// <summary>
        /// Update the list of project references according to user input, 
        /// i.e. need to exclude from the list references which user didn't select.
        /// </summary>
        void IReferencesViewModel.UpdateReferences()
        {
            var qry = this.Projects
                .Select(p => p)
                .Where(p => p.References.Any(r => !r.IsUnused))
                .Join(_solutionInspectResult,
                    projectData => projectData.Project,
                    inspectedProject => inspectedProject.Project,
                    (projectData, inspectedProject) => projectData.References
                        .Where(item => !item.IsUnused)
                        .Select(item => inspectedProject.UnusedReferences.Remove(item.ProjectReference)).Count()).Count();            
        }

        #endregion // IReferencesViewModel implementation

        #region Private methods

        private ObservableCollection<ProjectData> GetSolutionReferencesList()
        {
            return new ObservableCollection<ProjectData>
                (
                    _solutionInspectResult
                        .Where(item => (!item.IsSuccess || item.UnusedReferences.Count > 0))
                        .Select(item => new ProjectData(item))
                );
        }

        #endregion // Private methods
    }
}
