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
    public sealed class ProjectReference : IEquatable<ProjectReference>
    {
        private const string VersionField = "Version";
        private const string CultureField = "Culture";
        private const string PublicKeyTokenField = "PublicKeyToken";
        private const string NeutralCulture = "neutral";        
        private const string NullPublicKeyToken = "null";

        private string _fullName;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ProjectReference()
        { 
        }

        #region IEquatable Members

        /// <summary>
        /// Equals objects.
        /// </summary>
        /// <param name="other">Other object.</param>
        /// <returns>If true, then equals.</returns>
        public bool Equals(ProjectReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Compare(other.FullName, this.FullName, StringComparison.InvariantCultureIgnoreCase) == 0;
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

        /// <summary>
        /// Get hash code.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return this.FullName.GetHashCode();
        }

        #endregion

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
        public string Version { get; set; }

        /// <summary>
        /// Culture.
        /// </summary>
        public string Culture { get; set; }

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
                string name = Location.Contains(Identity) ? Identity : Name;
                return Path.IsPathRooted(name) ? Path.GetFileNameWithoutExtension(name) : name;
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
            builder.Append(CultureField).Append("=").Append(string.IsNullOrEmpty(Culture) ? NeutralCulture : Culture).Append(", ");
            builder.Append(PublicKeyTokenField).Append("=").Append(string.IsNullOrEmpty(PublicKeyToken) ? NullPublicKeyToken : PublicKeyToken.ToLower());

            return builder.ToString();
        }

        #endregion        
    }
}