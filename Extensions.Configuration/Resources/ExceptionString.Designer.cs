﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ichosoft.Extensions.Configuration.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ExceptionString {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ExceptionString() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Ichosoft.Extensions.Configuration.Resources.ExceptionString", typeof(ExceptionString).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is invalid for this provider. .
        /// </summary>
        internal static string Aes_InvalidSize {
            get {
                return ResourceManager.GetString("Aes.InvalidSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not decrypt value..
        /// </summary>
        internal static string EncryptionProvider_DecryptionFailed {
            get {
                return ResourceManager.GetString("EncryptionProvider.DecryptionFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not encrypt value..
        /// </summary>
        internal static string EncryptionProvider_EncryptionFailed {
            get {
                return ResourceManager.GetString("EncryptionProvider.EncryptionFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The {0} has not been set..
        /// </summary>
        internal static string EncryptionProvider_ProviderNotSet {
            get {
                return ResourceManager.GetString("EncryptionProvider.ProviderNotSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to complete key rotation..
        /// </summary>
        internal static string EncryptionProvider_RotateKeyFailed {
            get {
                return ResourceManager.GetString("EncryptionProvider.RotateKeyFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} cannot be assigned to; it is read-only..
        /// </summary>
        internal static string EncryptionProvider_SettingIsReadOnly {
            get {
                return ResourceManager.GetString("EncryptionProvider.SettingIsReadOnly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} could not be created..
        /// </summary>
        internal static string RSAKeyStore_CreateKeyContainerFailed {
            get {
                return ResourceManager.GetString("RSAKeyStore.CreateKeyContainerFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not delete {0}..
        /// </summary>
        internal static string RSAKeyStore_DeleteKeyContainerFailed {
            get {
                return ResourceManager.GetString("RSAKeyStore.DeleteKeyContainerFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is not supported for this operation..
        /// </summary>
        internal static string RSAKeyStore_PlatformNotSupported {
            get {
                return ResourceManager.GetString("RSAKeyStore.PlatformNotSupported", resourceCulture);
            }
        }
    }
}
