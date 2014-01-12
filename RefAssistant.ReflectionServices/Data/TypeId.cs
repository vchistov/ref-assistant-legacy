using System;
using System.Diagnostics.Contracts;

namespace Lardite.RefAssistant.ReflectionServices.Data
{
    [Serializable]
    public sealed class TypeId : IEquatable<TypeId>
    {
        private readonly string _fullName;
        private readonly AssemblyId _assemblyId;
        private readonly AssemblyId _forwardedFromId;

        private TypeId(string fullName, AssemblyId assemblyId, AssemblyId forwardedFromId)
        {
            ThrowUtils.ArgumentNullOrEmpty(() => fullName);
            ThrowUtils.ArgumentNull(() => assemblyId);

            _assemblyId = assemblyId;
            _fullName = fullName;
            _forwardedFromId = forwardedFromId;
        }

        internal static TypeId GetId(string fullName, AssemblyId assemblyId, AssemblyId forwardedFromId = null)
        {
            return new TypeId(fullName, assemblyId, forwardedFromId);
        }

        internal string FullName
        {
            get { return _fullName; }
        }

        internal AssemblyId AssemblyId
        {
            get { return _assemblyId; }
        }

        internal AssemblyId ForwardedFromId
        {
            get { return _forwardedFromId; }
        }

        public bool Equals(TypeId other)
        {
            if (other == null)
            {
                return false;
            }

            return string.Equals(this._fullName, other._fullName, StringComparison.Ordinal)
                && AssemblyId.Equals(this.AssemblyId, other.AssemblyId)
                && AssemblyId.Equals(this.ForwardedFromId, other.ForwardedFromId);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return ((IEquatable<TypeId>)this).Equals(obj as TypeId);
        }

        public static bool operator ==(TypeId a, TypeId b)
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

        public static bool operator !=(TypeId a, TypeId b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return _fullName.GetHashCode() ^ _assemblyId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", _fullName, _assemblyId.FullName);
        }
    }
}
