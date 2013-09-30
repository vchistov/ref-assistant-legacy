//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Windows;
using Lardite.RefAssistant.UI.ViewModel;
using Microsoft.VisualStudio.PlatformUI;
using Lardite.RefAssistant.ObjectModel;
using System.Linq;

namespace Lardite.RefAssistant.UI
{
    /// <summary>
    /// View and ajust list of ready to remove references.
    /// </summary>
    partial class UnusedReferencesWindow : DialogWindow
    {
        #region Fields

        private readonly IInspectResult _inspectResults;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="Lardite.RefAssistant.UI.UnusedReferencesWindow"/> class.
        /// </summary>
        public UnusedReferencesWindow(IInspectResult inspectResults)
        {
            if (inspectResults == null)
            {
                throw Error.ArgumentNull("inspectResults");
            }

            _inspectResults = inspectResults;
            this.UnusedReferencesViewModel = (inspectResults.InspectResults.Count() > 1)
                ? new SolutionReferencesViewModel(inspectResults.InspectResults) 
                : (IReferencesViewModel) new ProjectReferencesViewModel(inspectResults.InspectResults.First());

            InitializeComponent();
        }

        static UnusedReferencesWindow()
        {
            IsShowThisWindowAgainProperty = DependencyProperty.Register("IsShowThisWindowAgain",
                typeof(bool), typeof(UnusedReferencesWindow), new PropertyMetadata(true));

            UnusedReferencesViewModelPropertyKey = DependencyProperty.RegisterReadOnly("UnusedReferencesViewModelProperty", 
                typeof(IReferencesViewModel), typeof(UnusedReferencesWindow), new PropertyMetadata(null));

            UnusedReferencesViewModelProperty = UnusedReferencesViewModelPropertyKey.DependencyProperty;
        }

        #endregion // .ctor

        #region Dependency properties

        public static readonly DependencyProperty IsShowThisWindowAgainProperty;

        public static readonly DependencyProperty UnusedReferencesViewModelProperty;

        private static readonly DependencyPropertyKey UnusedReferencesViewModelPropertyKey;

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
        public IReferencesViewModel UnusedReferencesViewModel
        {
            get { return (IReferencesViewModel)GetValue(UnusedReferencesViewModelProperty); }
            private set { SetValue(UnusedReferencesViewModelPropertyKey, value); }
        }

        #endregion // Properties

        #region Events handlers

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult.HasValue && this.DialogResult.Value)
            {
                this.UnusedReferencesViewModel.UpdateReferences();
            }
            base.OnClosing(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        #endregion // Events handlers
    }
}
