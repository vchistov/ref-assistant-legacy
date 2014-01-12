using System;
using System.Diagnostics.Contracts;

namespace Lardite.RefAssistant.ReflectionServices.Data
{
    [Serializable]
    public sealed class AssemblyId : IEquatable<AssemblyId>
    {
        private readonly string _fullName;

        private AssemblyId(string fullName)
        {
            ThrowUtils.ArgumentNullOrEmpty(() => fullName);
            _fullName = fullName;
        }

        internal static AssemblyId GetId(string fullName)
        {
            return new AssemblyId(fullName);
        }

        internal string FullName
        {
            get { return _fullName; }
        }

        public bool Equals(AssemblyId other)
        {
            if (other == null)
            {
                return false;
            }

            return string.Equals(this._fullName, other._fullName, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return ((IEquatable<AssemblyId>)this).Equals(obj as AssemblyId);
        }

        public static bool operator ==(AssemblyId a, AssemblyId b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(AssemblyId a, AssemblyId b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return _fullName.GetHashCode();
        }

        public override string ToString()
        {
            return _fullName;
        }
    }
}
