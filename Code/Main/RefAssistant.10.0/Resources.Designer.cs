﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.225
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Lardite.RefAssistant {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Lardite.RefAssistant.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The target must be a boolean..
        /// </summary>
        public static string InvertBooleanConverter_InvalidOperation {
            get {
                return ResourceManager.GetString("InvertBooleanConverter_InvalidOperation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error.
        /// </summary>
        public static string OutputLog_Error {
            get {
                return ResourceManager.GetString("OutputLog_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Warning.
        /// </summary>
        public static string OutputLog_Warning {
            get {
                return ResourceManager.GetString("OutputLog_Warning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can not create window to show removed references list..
        /// </summary>
        public static string RefAssistantPackage_CanNotCreateWindow {
            get {
                return ResourceManager.GetString("RefAssistantPackage_CanNotCreateWindow", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ========== Remove: failed ==========.
        /// </summary>
        public static string RefAssistantPackage_EndProcessFailed {
            get {
                return ResourceManager.GetString("RefAssistantPackage_EndProcessFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error has occurred while deleting unused references..
        /// </summary>
        public static string RefAssistantPackage_ErrorOccured {
            get {
                return ResourceManager.GetString("RefAssistantPackage_ErrorOccured", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Project is not supported..
        /// </summary>
        public static string RefAssistantPackage_ProjectIsNotSupported {
            get {
                return ResourceManager.GetString("RefAssistantPackage_ProjectIsNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove unused usings.
        /// </summary>
        public static string RefAssistantPackage_RemoveUnusedUsings {
            get {
                return ResourceManager.GetString("RefAssistantPackage_RemoveUnusedUsings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Start removing unused references. A project kind is &apos;{0}&apos;..
        /// </summary>
        public static string RefAssistantPackage_StartRemoving {
            get {
                return ResourceManager.GetString("RefAssistantPackage_StartRemoving", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Need to specify the unused references list before show the window..
        /// </summary>
        public static string RefAssistantPackage_UnusedReferencesAreNotDefined {
            get {
                return ResourceManager.GetString("RefAssistantPackage_UnusedReferencesAreNotDefined", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove unused Usings after removing.
        /// </summary>
        public static string UI_GeneralOptionsControl_CheckBoxRemoveUsings_Content {
            get {
                return ResourceManager.GetString("UI_GeneralOptionsControl_CheckBoxRemoveUsings_Content", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Apply the Remove Unused Using operation to all project files..
        /// </summary>
        public static string UI_GeneralOptionsControl_CheckBoxRemoveUsings_Help {
            get {
                return ResourceManager.GetString("UI_GeneralOptionsControl_CheckBoxRemoveUsings_Help", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show Unused References Window before removing.
        /// </summary>
        public static string UI_GeneralOptionsControl_CheckBoxShowWindow_Content {
            get {
                return ResourceManager.GetString("UI_GeneralOptionsControl_CheckBoxShowWindow_Content", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show the window containing list of removable references. Each of these references can be excluded from removable references..
        /// </summary>
        public static string UI_GeneralOptionsControl_CheckBoxShowWindow_Help {
            get {
                return ResourceManager.GetString("UI_GeneralOptionsControl_CheckBoxShowWindow_Help", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cancel.
        /// </summary>
        public static string UI_UnusedReferencesWindow_Cancel_Content {
            get {
                return ResourceManager.GetString("UI_UnusedReferencesWindow_Cancel_Content", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Check or uncheck assembly to include or exclude it from unused references list.
        /// </summary>
        public static string UI_UnusedReferencesWindow_CheckReference_ToolTip {
            get {
                return ResourceManager.GetString("UI_UnusedReferencesWindow_CheckReference_ToolTip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Don&apos;t show this dialog again in next time.
        /// </summary>
        public static string UI_UnusedReferencesWindow_DontShowDialogAgain_Content {
            get {
                return ResourceManager.GetString("UI_UnusedReferencesWindow_DontShowDialogAgain_Content", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OK.
        /// </summary>
        public static string UI_UnusedReferencesWindow_Ok_Content {
            get {
                return ResourceManager.GetString("UI_UnusedReferencesWindow_Ok_Content", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to These references can be removed:.
        /// </summary>
        public static string UI_UnusedReferencesWindow_RemovableReferences_Text {
            get {
                return ResourceManager.GetString("UI_UnusedReferencesWindow_RemovableReferences_Text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unused References List.
        /// </summary>
        public static string UI_UnusedReferencesWindow_Title {
            get {
                return ResourceManager.GetString("UI_UnusedReferencesWindow_Title", resourceCulture);
            }
        }
    }
}
