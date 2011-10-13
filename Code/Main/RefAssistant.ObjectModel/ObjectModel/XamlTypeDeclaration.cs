//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xaml;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// Type that declared into a XAML markup.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    sealed class XamlTypeDeclaration : IEquatable<XamlTypeDeclaration>
    {
        #region Fields

        private readonly XamlType _xamlType;
        private readonly string _assemblyName;
        private readonly string _name;
        private readonly string _namespace;
        private readonly static XamlTypeDeclarationComparer _comparer = new XamlTypeDeclarationComparer();

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Lardite.RefAssistant.ObjectModel.XamlTypeDeclaration"/> class.
        /// </summary>
        /// <param name="xamlType">The type information.</param>
        public XamlTypeDeclaration(XamlType xamlType)            
        {
            if (xamlType == null)
            {
                throw Error.ArgumentNull("xamlType");
            }

            _xamlType = xamlType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lardite.RefAssistant.ObjectModel.XamlTypeDeclaration"/> class.
        /// </summary>
        /// <param name="typeNamespace">The namespace of the type.</param>
        /// <param name="name">The name of the type.</param>
        public XamlTypeDeclaration(string typeNamespace, string name)
            : this(null, typeNamespace, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lardite.RefAssistant.ObjectModel.XamlTypeDeclaration"/> class.
        /// </summary>
        /// <param name="assemblyName">The assembly name when type is defined.</param>
        /// <param name="typeNamespace">The namespace of the type.</param>
        /// <param name="name">The name of the type.</param>
        public XamlTypeDeclaration(string assemblyName, string typeNamespace, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw Error.ArgumentNull("name");
            }

            _assemblyName = assemblyName;
            _namespace = typeNamespace;
            _name = name;
        }
        
        #endregion // .ctor

        #region Properties

        /// <summary>
        /// Get type name.
        /// </summary>
        public string Name
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_name))
                {
                    return _name;
                }

                return (_xamlType != null) ? _xamlType.Name : string.Empty;                    
            }
        }

        /// <summary>
        /// Get type namespace.
        /// </summary>
        public string Namespace
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PreferredXamlNamespace))
                {
                    return string.Empty;
                }

                if (IsClrNamespace)
                {
                    var regex = new Regex(@"clr-namespace:\s*(?<namespace>[^;]+)", RegexOptions.IgnoreCase);
                    var match = regex.Match(PreferredXamlNamespace);
                    if (match.Success)
                    {
                        return match.Groups["namespace"].Value;
                    }
                }

                return PreferredXamlNamespace;
            }
        }

        /// <summary>
        /// Get type namespace.
        /// </summary>
        public string PreferredXamlNamespace
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_namespace))
                {
                    return _namespace;
                }

                return (_xamlType != null) ? _xamlType.PreferredXamlNamespace : string.Empty;
            }
        }

        /// <summary>
        /// Get assembly name when type is defined.
        /// </summary>
        public string AssemblyName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_assemblyName))
                {
                    return _assemblyName;
                }

                if (_xamlType != null && !_xamlType.IsUnknown)
                {
                    return _xamlType.UnderlyingType.Assembly.FullName;
                }

                if (IsClrNamespace)
                {
                    var regex = new Regex(@"assembly=\s*(?<assembly>[^,;]+)", RegexOptions.IgnoreCase);
                    var match = regex.Match(PreferredXamlNamespace);
                    if (match.Success)
                    {
                        return match.Groups["assembly"].Value;
                    }
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Get name of the type combined with assembly name.
        /// </summary>
        public string AssemblyQualifiedName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Name) 
                    && ((!string.IsNullOrWhiteSpace(Namespace) && IsClrNamespace) || string.IsNullOrWhiteSpace(Namespace))                    
                    && !string.IsNullOrWhiteSpace(AssemblyName))
                {
                    var sb = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(Namespace))
                    {
                        sb.Append(Namespace).Append(".");
                    }
                    
                    return sb.Append(Name).Append(", ").Append(AssemblyName).ToString();
                }

                return (_xamlType != null) ?
                    ((!_xamlType.IsUnknown) ? _xamlType.UnderlyingType.AssemblyQualifiedName : string.Empty)
                    : string.Empty;
            }
        }

        /// <summary>
        /// Indicates if namespace is in XML fashion, not CLR.
        /// </summary>
        public bool IsXmlNamespace
        {
            get
            {
                return PreferredXamlNamespace.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                    || PreferredXamlNamespace.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Indicates if namespace is in CLR fashion, not XML.
        /// </summary>
        public bool IsClrNamespace
        {
            get
            {
                return PreferredXamlNamespace.StartsWith("clr-namespace:", StringComparison.InvariantCultureIgnoreCase)
                    || (!string.IsNullOrWhiteSpace(PreferredXamlNamespace) && !IsXmlNamespace);
            }
        }

        /// <summary>
        /// Get comparer for the <see cref="XamlTypeDeclaration"/> type.
        /// </summary>
        public static XamlTypeDeclarationComparer Comparer
        {
            get
            {
                return _comparer;
            }
        }

        #endregion // Properties

        #region IEquatable implementation

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.        
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(XamlTypeDeclaration obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }

            var result = (_xamlType != null && _xamlType == obj._xamlType) ||
                ((string.Compare(Name, obj.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                && (string.Compare(PreferredXamlNamespace, obj.PreferredXamlNamespace, StringComparison.InvariantCultureIgnoreCase) == 0)
                && (string.Compare(AssemblyName, obj.AssemblyName, StringComparison.InvariantCultureIgnoreCase) == 0));

            return result;
        }

        #endregion // IEquatable implementation

        #region Nested types

        /// <summary>
        /// The <see cref="XamlTypeDeclaration"/> comparer.
        /// </summary>
        public sealed class XamlTypeDeclarationComparer : IEqualityComparer<XamlTypeDeclaration>
        {
            #region IEqualityComparer implementation

            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first object of type <see cref="Lardite.RefAssistant.ObjectModel.Checkers.XamlTypeDeclaration"/> to compare.</param>
            /// <param name="y">The second object of type <see cref="Lardite.RefAssistant.ObjectModel.Checkers.XamlTypeDeclaration"/> to compare.</param>
            /// <returns>true if the specified objects are equal; otherwise, false.</returns>
            public bool Equals(XamlTypeDeclaration obj1, XamlTypeDeclaration obj2)
            {
                return obj1.Equals(obj2);
            }

            /// <summary>
            /// Returns a hash code for the specified object.
            /// </summary>
            /// <param name="obj">The Object for which a hash code is to be returned.</param>
            /// <returns>A hash code for the specified object.</returns>
            public int GetHashCode(XamlTypeDeclaration obj)
            {
                // Check whether the object is null.
                if (Object.ReferenceEquals(obj, null))
                {
                    return 0;
                }

                if (obj._xamlType != null)
                {
                    return obj._xamlType.GetHashCode();
                }

                int hashAssembly = string.IsNullOrEmpty(obj.AssemblyName) ? 0 : obj.AssemblyName.GetHashCode();
                int hashName = string.IsNullOrEmpty(obj.Name) ? 0 : obj.Name.GetHashCode();
                int hashNamespace = string.IsNullOrEmpty(obj.PreferredXamlNamespace) ? 0 : obj.PreferredXamlNamespace.GetHashCode();


                return hashAssembly ^ hashName ^ hashNamespace;
            }

            #endregion // IEqualityComparer implementation
        }

        #endregion // Nested types
    }    
}
