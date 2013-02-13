//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.Windows;
using System.Windows.Controls;

namespace Lardite.RefAssistant.UI
{
    /// <summary>
    /// UI for controlling extesion settings.
    /// </summary>
    partial class GeneralOptionsControl : UserControl
    {
        #region Fields

        private readonly GeneralOptionsPage _generalOptionsPage;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Lardite.RefAssistant.UI.GeneralOptionsControl"/> class.
        /// </summary>
        /// <param name="generalOptionPage">Extension's options page.</param>
        public GeneralOptionsControl(GeneralOptionsPage generalOptionPage)
        {
            _generalOptionsPage = generalOptionPage;
            InitializeComponent();
        }

        static GeneralOptionsControl()
        {
            IsShowUnusedReferencesWindowProperty = DependencyProperty.Register("IsShowUnusedReferencesWindow", 
                typeof(bool?), typeof(GeneralOptionsControl), 
                new PropertyMetadata(true, OnShowUnusedReferencesWindowChanged));

            IsRemoveUsingsAfterRemovingProperty = DependencyProperty.Register("IsRemoveUsingsAfterRemoving",
                typeof(bool?), typeof(GeneralOptionsControl),
                new PropertyMetadata(false, OnRemoveUsingsAfterRemovingChanged));
        }

        #endregion // .ctor

        #region Dependencies properties

        public static readonly DependencyProperty IsShowUnusedReferencesWindowProperty;

        public static readonly DependencyProperty IsRemoveUsingsAfterRemovingProperty;

        #endregion // Dependencies properties

        #region Properties

        public bool? IsShowUnusedReferencesWindow
        {
            get { return (bool?)GetValue(IsShowUnusedReferencesWindowProperty); }
            set { SetValue(IsShowUnusedReferencesWindowProperty, value); }
        }

        public bool? IsRemoveUsingsAfterRemoving
        {
            get { return (bool?)GetValue(IsRemoveUsingsAfterRemovingProperty); }
            set { SetValue(IsRemoveUsingsAfterRemovingProperty, value); }
        }

        #endregion // Properties

        #region Events handlers

        private static void OnShowUnusedReferencesWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((GeneralOptionsControl)d)._generalOptionsPage.IsShowUnusedReferencesWindow = (bool?)args.NewValue;
        }

        private static void OnRemoveUsingsAfterRemovingChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((GeneralOptionsControl)d)._generalOptionsPage.IsRemoveUsingsAfterRemoving = (bool?)args.NewValue;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            IsShowUnusedReferencesWindow = _generalOptionsPage.IsShowUnusedReferencesWindow;
            IsRemoveUsingsAfterRemoving = _generalOptionsPage.IsRemoveUsingsAfterRemoving;
        }

        #endregion // Events handlers    
    }
}
