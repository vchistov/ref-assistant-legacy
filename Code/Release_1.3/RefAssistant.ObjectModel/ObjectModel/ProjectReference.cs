//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// Contains information about project reference.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{FullName}")]
    public sealed class ProjectReference : IEquatable<ProjectReference>, IComparable<string>
    {
        #region Fields

        private const string VersionField = "Version";
        private const string CultureField = "Culture";
        private const string PublicKeyTokenField = "PublicKeyToken";
        private const string NeutralCulture = "neutral";        
        private const string NullPublicKeyToken = "null";

        private string _fullName;
        private string _culture;

        #endregion // Fields
        
        #region Object overrides

        /// <summary>
        /// Get hash code.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return FullName.ToUpper().GetHashCode();
        }

        /// <summary>
        /// Convert object to string.
        /// </summary>
        /// <returns>Returns object string presentation.</returns>
        public override string ToString()
        {
            return FullName;
        }

        #endregion // Object overrides

        #region Properties

        /// <summary>
        /// Get or set the project reference name. This name can be different that assembly name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Identity.
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// Location.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Version.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Culture.
        /// </summary>
        public string Culture 
        {
            get
            {
                return string.IsNullOrWhiteSpace(_culture) ? NeutralCulture : _culture;
            }
            set
            {
                _culture = value;
            }
        }

        /// <summary>
        /// Public key token
        /// </summary>
        public string PublicKeyToken { get; set; }

        /// <summary>
        /// Get the assembly name.
        /// </summary>
        public string AssemblyName
        {
            get
            {
                var physicalName = Path.GetFileNameWithoutExtension(Location);
                string assemblyName = physicalName.Equals(Identity, StringComparison.OrdinalIgnoreCase) 
                    ? Identity
                    : physicalName;

                return assemblyName;
            }
        }

        /// <summary>
        /// Full name.
        /// </summary>
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(_fullName))
                {
                    _fullName = BuildFullName();
                }
                return _fullName;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Builds full name of the project reference.
        /// </summary>
        /// <returns>Full name.</returns>
        private string BuildFullName()
        {
            StringBuilder builder = new StringBuilder(AssemblyName).Append(", ");
            builder.Append(VersionField).Append("=").Append(Version).Append(", ");
            builder.Append(CultureField).Append("=").Append(Culture).Append(", ");
            builder.Append(PublicKeyTokenField).Append("=").Append(string.IsNullOrWhiteSpace(PublicKeyToken) ? NullPublicKeyToken : PublicKeyToken.ToLower());

            return builder.ToString();
        }

        #endregion        

        #region IEquatable implementation

        /// <summary>
        /// Equals objects.
        /// </summary>
        /// <param name="other">Other object.</param>
        /// <returns>If true, then equals.</returns>
        public bool Equals(ProjectReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return CompareTo(other.FullName) == 0;
        }

        /// <summary>
        /// Equals objects.
        /// </summary>
        /// <param name="obj">Other object.</param>
        /// <returns>If true, then equals.</returns>
        public override bool Equals(Object obj)
        {
            if (obj == null) return base.Equals(obj);

            return (obj is ProjectReference)
                ? Equals(obj as ProjectReference)
                : false;
        }

        #endregion

        #region IComparable implementation

        /// <summary>
        /// Compare this instance with string of full name of an assembly.
        /// </summary>
        /// <param name="assemblyFullName">An object to compare with this instance. </param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings: &lt;0 - this instance precedes <paramref name="assemblyFullName"/> in the sort order; 0 - This instance occurs in the same position in the sort order as <paramref name="assemblyFullName"/>; &gt;0 - this instance follows <paramref name="assemblyFullName"/> in the sort order.</returns>
        public int CompareTo(string assemblyFullName)
        {
            return string.Compare(FullName, assemblyFullName, StringComparison.OrdinalIgnoreCase);
        }

        #endregion // IComparable implementation
    }
}
