﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Wist.Node.Core.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Wist.Node.Core.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Consensus on Chain Type {0} is not supported..
        /// </summary>
        internal static string ERR_CONSENSUS_ON_CHAINTYPE_NOT_SUPPORTED {
            get {
                return ResourceManager.GetString("ERR_CONSENSUS_ON_CHAINTYPE_NOT_SUPPORTED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DPOS Provider for PacketType {0} is not supported..
        /// </summary>
        internal static string ERR_DPOS_PROVIDER_NOT_SUPPORTED {
            get {
                return ResourceManager.GetString("ERR_DPOS_PROVIDER_NOT_SUPPORTED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MemPool of elements of type &apos;{0}&apos; is not supported..
        /// </summary>
        internal static string ERR_MEMPOOL_OF_ELEMENTS_NOT_SUPPORTED {
            get {
                return ResourceManager.GetString("ERR_MEMPOOL_OF_ELEMENTS_NOT_SUPPORTED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Role &apos;{0}&apos; is not supported..
        /// </summary>
        internal static string ERR_ROLE_NOT_SUPPORTED {
            get {
                return ResourceManager.GetString("ERR_ROLE_NOT_SUPPORTED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Argument passed to function must be of type &apos;{0}&apos;..
        /// </summary>
        internal static string ERR_UNEXPECTED_ARGUMENT_TYPE {
            get {
                return ResourceManager.GetString("ERR_UNEXPECTED_ARGUMENT_TYPE", resourceCulture);
            }
        }
    }
}
