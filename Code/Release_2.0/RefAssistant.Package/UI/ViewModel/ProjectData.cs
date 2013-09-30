//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

using Lardite.RefAssistant.ObjectModel;

namespace Lardite.RefAssistant.UI.ViewModel
{
    /// <summary>
    /// Passing the project's references to the view model.
    /// </summary>
    internal sealed class ProjectData : DependencyObject
    {
        #region Fields

        private readonly IProjectInspectResult _projectInspectResult;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="ProjectData"/> class.
        /// </summary>
        /// <param name="projectInspectResult">The information about project.</param>
        public ProjectData(IProjectInspectResult projectInspectResult)
        {
            if (projectInspectResult == null)
            {
                throw Error.ArgumentNull("projectInspectResult");
            }

            _projectInspectResult = projectInspectResult;
            this.ErrorMessage = (!projectInspectResult.IsSuccess) ? projectInspectResult.Exception.Message : null;
            this.References = new ReadOnlyObservableCollection<ReferenceData>
                (
                    new ObservableCollection<ReferenceData>(projectInspectResult.UnusedReferences.Select(item => new ReferenceData(item)))
                );
        }

        /// <summary>
        /// Initialize the dependency properties.
        /// </summary>
        static ProjectData()
        {
            ReferencesPropertyKey = DependencyProperty.RegisterReadOnly("References",
                typeof(ReadOnlyObservableCollection<ReferenceData>), typeof(ProjectData), new PropertyMetadata(null));

            ReferencesProperty = ReferencesPropertyKey.DependencyProperty;

            ErrorMessagePropertyKey = DependencyProperty.RegisterReadOnly("ErrorMessage",
                typeof(string), typeof(ProjectData), new PropertyMetadata(null));

            ErrorMessageProperty = ErrorMessagePropertyKey.DependencyProperty;
        }

        #endregion // .ctor

        #region Dependency proprties

        private static readonly DependencyPropertyKey ReferencesPropertyKey;
        public static readonly DependencyProperty ReferencesProperty;

        private static readonly DependencyPropertyKey ErrorMessagePropertyKey;
        public static readonly DependencyProperty ErrorMessageProperty;

        #endregion // Dependency proprties

        #region Properties

        /// <summary>
        /// Get the information about project.
        /// </summary>
        public ProjectInfo Project
        {
            get { return _projectInspectResult.Project; }
        }        

        /// <summary>
        /// Get the list of project's references. This is dependency property.
        /// </summary>
        public ReadOnlyObservableCollection<ReferenceData> References
        {
            get { return (ReadOnlyObservableCollection<ReferenceData>)GetValue(ReferencesProperty); }
            private set { SetValue(ReferencesPropertyKey, value); }
        }

        /// <summary>
        /// Get the error message. This is dependency property.
        /// </summary>
        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            private set { SetValue(ErrorMessagePropertyKey, value); }
        }

        #endregion // Properties        
    }
}
