//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Linq;
using System.Windows;

using Lardite.RefAssistant.ObjectModel;

namespace Lardite.RefAssistant.UI.ViewModel
{
    internal sealed class ProjectReferencesViewModel : DependencyObject, IReferencesViewModel
    {
        #region Fields

        private readonly IProjectInspectResult _projectInspectResult;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="ProjectReferencesViewModel"/> class.
        /// </summary>
        /// <param name="projectInspectResult">The project's unused references.</param>
        public ProjectReferencesViewModel(IProjectInspectResult projectInspectResult)
        {
            if (projectInspectResult == null)
            {
                throw Error.ArgumentNull("projectInspectResult");
            }

            _projectInspectResult = projectInspectResult;
            this.Project = new ProjectData(projectInspectResult);
        }

        #endregion // .ctor

        #region Properties

        public ProjectData Project
        {
            get;
            private set;
        }

        #endregion // Properties

        #region IReferencesViewModel implementation

        /// <summary>
        /// Update the list of project references according to user input, 
        /// i.e. need to exclude from the list references which user didn't select.
        /// </summary>
        void IReferencesViewModel.UpdateReferences()
        {
            this.Project.References
                .Where(item => !item.IsUnused)
                .Select(item => _projectInspectResult.UnusedReferences.Remove(item.ProjectReference))
                .Count();
        }

        #endregion // IReferencesViewModel implementation        
    }
}
