//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;
using System.Windows;
using Lardite.RefAssistant.Model.Projects;
using Lardite.RefAssistant.UI.ViewModel;
using Microsoft.VisualStudio.PlatformUI;

namespace Lardite.RefAssistant.UI
{
    /// <summary>
    /// View and ajust list of ready to remove references.
    /// </summary>
    partial class UnusedReferencesWindow : DialogWindow
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="Lardite.RefAssistant.UI.UnusedReferencesWindow"/> class.
        /// </summary>
        public UnusedReferencesWindow(IEnumerable<VsProjectReference> references)
        {
            ThrowUtils.ArgumentNull(() => references);

            this.ViewModel = new ProjectReferencesViewModel(references);                

            InitializeComponent();
        }

        static UnusedReferencesWindow()
        {
            IsShowThisWindowAgainProperty = DependencyProperty.Register("IsShowThisWindowAgain",
                typeof(bool), typeof(UnusedReferencesWindow), new PropertyMetadata(true));

            ViewModelPropertyKey = DependencyProperty.RegisterReadOnly("ViewModelProperty", 
                typeof(IReferencesViewModel), typeof(UnusedReferencesWindow), new PropertyMetadata(null));

            ViewModelProperty = ViewModelPropertyKey.DependencyProperty;
        }

        #region Dependency properties

        public static readonly DependencyProperty IsShowThisWindowAgainProperty;

        public static readonly DependencyProperty ViewModelProperty;

        private static readonly DependencyPropertyKey ViewModelPropertyKey;

        #endregion // Dependency properties

        #region Properties

        /// <summary>
        /// Get or set whether to show this dialog again in next time.
        /// </summary>
        public bool IsShowThisWindowAgain
        {
            get { return (bool)GetValue(IsShowThisWindowAgainProperty); }
            set { SetValue(IsShowThisWindowAgainProperty, value); }
        }

        /// <summary>
        /// Get project's unused references view model.
        /// </summary>
        public IReferencesViewModel ViewModel
        {
            get { return (IReferencesViewModel)GetValue(ViewModelProperty); }
            private set { SetValue(ViewModelPropertyKey, value); }
        }

        #endregion // Properties

        #region Events handlers

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        #endregion // Events handlers
    }
}