//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Windows;

using Lardite.RefAssistant.ObjectModel;

namespace Lardite.RefAssistant.UI.ViewModel
{
    /// <summary>
    /// Passing the reference to the view model.
    /// </summary>
    internal sealed class ReferenceData : DependencyObject
    {
        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="ReferenceData"/> class.
        /// </summary>
        /// <param name="projectReference">The assembly reference.</param>
        public ReferenceData(ProjectReference projectReference)
        {
            IsUnused = true;
            ProjectReference = projectReference;
        }

        /// <summary>
        /// Initialize the dependency properties.
        /// </summary>
        static ReferenceData()
        {
            IsUnusedProperty = DependencyProperty.Register("IsUnused", typeof(bool),
                typeof(ReferenceData), new UIPropertyMetadata(true));

            ProjectReferencePropertyKey = DependencyProperty.RegisterReadOnly("ProjectReference",
                typeof(ProjectReference), typeof(ReferenceData), new UIPropertyMetadata(null));
            ProjectReferenceProperty = ProjectReferencePropertyKey.DependencyProperty;
        }

        #endregion // .ctor

        #region Dependency properties

        public static readonly DependencyProperty IsUnusedProperty;

        private static readonly DependencyPropertyKey ProjectReferencePropertyKey;
        public static readonly DependencyProperty ProjectReferenceProperty;

        #endregion // Dependency properties

        #region Properties
        
        /// <summary>
        /// Defines whether to save a link to remove. This is dependency property.
        /// </summary>
        public bool IsUnused
        {
            get { return (bool)GetValue(IsUnusedProperty); }
            set { SetValue(IsUnusedProperty, value); }
        }

        /// <summary>
        /// The assembly name.
        /// </summary>
        public string AssemblyName
        {
            get { return ProjectReference.Name; }
        }

        /// <summary>
        /// The assembly full name.
        /// </summary>
        public string AssemblyFullName
        {
            get { return ProjectReference.FullName; }
        }

        /// <summary>
        /// The reference. This is dependency property.
        /// </summary>
        public ProjectReference ProjectReference
        {
            get { return (ProjectReference)GetValue(ProjectReferenceProperty); }
            private set { SetValue(ProjectReferencePropertyKey, value); }
        }

        #endregion // Properties
    }
}
