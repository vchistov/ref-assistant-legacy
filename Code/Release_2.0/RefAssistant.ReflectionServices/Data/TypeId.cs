using System;
using System.Diagnostics.Contracts;

namespace Lardite.RefAssistant.ReflectionServices.Data
{
    [Serializable]
    public sealed class TypeId : BaseNamedId, IEquatable<TypeId>
    {
        private readonly AssemblyId _assemblyId;
        private readonly AssemblyId _forwardedFromId;

        private TypeId(string fullName, AssemblyId assemblyId, AssemblyId forwardedFromId)
            : base(fullName)
        {
            ThrowUtils.ArgumentNull(() => assemblyId);

            _assemblyId = assemblyId;
            _forwardedFromId = forwardedFromId;
        }

        internal static TypeId GetId(string fullName, AssemblyId assemblyId, AssemblyId forwardedFromId = null)
        {
            return new TypeId(fullName, assemblyId, forwardedFromId);
        }

        internal string FullName
        {
            get { return _name; }
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
            if (!base.Equals(other))
                return false;

            return AssemblyId.Equals(this.AssemblyId, other.AssemblyId)
                && AssemblyId.Equals(this.ForwardedFromId, other.ForwardedFromId);
        }        

        public override bool Equals(object obj)
        {
            return this.Equals(obj as TypeId);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() 
                ^ _assemblyId.GetHashCode() 
                ^ (_forwardedFromId == null ? 0 : _forwardedFromId.GetHashCode());
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.FullName, _assemblyId.FullName);
        }
    }
}
