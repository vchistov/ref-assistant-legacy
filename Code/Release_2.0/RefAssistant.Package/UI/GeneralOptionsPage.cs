//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.VisualStudio.Shell;

namespace Lardite.RefAssistant.UI
{
    /// <summary>
    /// General settings of the extension.
    /// </summary>
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("26BE4169-E6F8-49F3-8122-02D1C5DE17BF")]
    sealed class GeneralOptionsPage : DialogPage, IExtensionOptions
    {
        #region Fields

        private ElementHost _control;
        
        #endregion 

        #region Properties

        [Category("General")]
        [DisplayName("Show Unused References Window before removing")]
        [Description("Show the window containing list of removable references. Each of these references can be excluded from removable references.")]
        [DefaultValue(true)]
        public bool? IsShowUnusedReferencesWindow { get; set; }

        [Category("General")]
        [DisplayName("Remove Unused Usings after removing")]
        [Description("Apply the Remove Unused Using operation to all project files.")]
        [DefaultValue(false)]
        public bool? IsRemoveUsingsAfterRemoving { get; set; }

        #endregion // Properties

        #region Overrides

        /// <summary>
        /// The window this dialog page will use for its UI.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get
            {
                if (_control == null)
                {
                    _control = new ElementHost();
                    _control.Child = new GeneralOptionsControl(this);
                }
                return (IWin32Window)_control;
            }
        }

        /// <summary>
        /// Load extension options.
        /// </summary>
        public override void LoadSettingsFromStorage()
        {
            base.LoadSettingsFromStorage();

            if (!IsInitialized)
            {
                SetDefaultSettings();
            }
        }

        /// <summary>
        /// Reset extension options.
        /// </summary>
        public override void ResetSettings()
        {
            SetDefaultSettings();
        }

        #endregion // Overrides

        #region Private methods

        private bool IsInitialized
        {
            get
            {
                return IsShowUnusedReferencesWindow.HasValue && IsRemoveUsingsAfterRemoving.HasValue;
            }
        }

        private void SetDefaultSettings()
        {
            IsShowUnusedReferencesWindow = true;
            IsRemoveUsingsAfterRemoving = false;
        }

        #endregion // Private methods
    }
}
