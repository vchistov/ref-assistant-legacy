//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Lardite.RefAssistant.ObjectModel;
using Microsoft.VisualStudio.PlatformUI;

namespace Lardite.RefAssistant.UI
{
    /// <summary>
    /// View and ajust list of ready to remove references.
    /// </summary>
    partial class UnusedReferencesWindow : DialogWindow
    {
        #region Fields

        private ObservableCollection<ProjectReferenceViewModel> _projectReferences;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="Lardite.RefAssistant.UI.UnusedReferencesWindow"/> class.
        /// </summary>
        public UnusedReferencesWindow()
        {
            _projectReferences = new ObservableCollection<ProjectReferenceViewModel>();

            InitializeComponent();
        }

        static UnusedReferencesWindow()
        {
            IsShowThisWindowAgainProperty = DependencyProperty.Register("IsShowThisWindowAgain",
                typeof(bool), typeof(UnusedReferencesWindow), new PropertyMetadata(true));
        }

        #endregion // .ctor

        #region Dependency properties

        public static readonly DependencyProperty IsShowThisWindowAgainProperty;

        #endregion // Dependency properties

        #region Properties

        /// <summary>
        /// Get list of the project references.
        /// </summary>
        public ReadOnlyObservableCollection<ProjectReferenceViewModel> ProjectReferencesList
        {
            get
            {
                return new ReadOnlyObservableCollection<ProjectReferenceViewModel>(_projectReferences);
            }
        }

        /// <summary>
        /// Get or set whether to show this dialog again in next time.
        /// </summary>
        public bool IsShowThisWindowAgain
        {
            get { return (bool)GetValue(IsShowThisWindowAgainProperty); }
            set { SetValue(IsShowThisWindowAgainProperty, value); }
        }           

        #endregion // Properties

        #region Public methods

        /// <summary>
        /// Set list of project references which can be removed.
        /// </summary>
        /// <param name="projectReferences">Project references list.</param>
        public void SetProjectReferences(IEnumerable<ProjectReference> projectReferences)
        {
            _projectReferences.Clear();
            foreach (var item in projectReferences)
            {
                _projectReferences.Add(new ProjectReferenceViewModel(item));
            }
        }

        /// <summary>
        /// Get list of the unused project references.
        /// </summary>
        public IEnumerable<ProjectReference> GetUnusedReferences()
        {
            return (from projectRef in ProjectReferencesList
                    where projectRef.IsUnused
                    select projectRef.ProjectReference).ToList();
        }

        #endregion // Public methods

        #region Events handlers

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        #endregion // Events handlers

        #region Nested types

        public class ProjectReferenceViewModel : DependencyObject
        {
            #region .ctor

            public ProjectReferenceViewModel(ProjectReference projectReference)
            {
                IsUnused = true;
                ProjectReference = projectReference;
            }

            static ProjectReferenceViewModel()
            {
                IsUnusedProperty = DependencyProperty.Register("IsUnused", typeof(bool),
                    typeof(ProjectReferenceViewModel), new UIPropertyMetadata(true));

                ProjectReferencePropertyKey = DependencyProperty.RegisterReadOnly("ProjectReference",
                    typeof(ProjectReference), typeof(ProjectReferenceViewModel), new UIPropertyMetadata(null));
                ProjectReferenceProperty = ProjectReferencePropertyKey.DependencyProperty;
            }

            #endregion // .ctor

            #region Properties

            public static readonly DependencyProperty IsUnusedProperty;
            public bool IsUnused
            {
                get { return (bool)GetValue(IsUnusedProperty); }
                set { SetValue(IsUnusedProperty, value); }
            }

            public string AssemblyName
            {
                get { return ProjectReference.Name; }
            }

            public string AssemblyFullName
            {
                get { return ProjectReference.FullName; }
            }

            private static readonly DependencyPropertyKey ProjectReferencePropertyKey;
            public static readonly DependencyProperty ProjectReferenceProperty;
            public ProjectReference ProjectReference
            {
                get { return (ProjectReference)GetValue(ProjectReferenceProperty); }
                private set { SetValue(ProjectReferencePropertyKey, value); }
            }

            #endregion // Properties
        }

        #endregion // Nested types
    }
}
