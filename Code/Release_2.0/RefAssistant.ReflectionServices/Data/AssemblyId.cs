using System;

namespace Lardite.RefAssistant.ReflectionServices.Data
{
    [Serializable]
    public sealed class AssemblyId : BaseNamedId, IEquatable<AssemblyId>
    {
        private AssemblyId(string fullName)
            : base(fullName)
        {
        }

        internal static AssemblyId GetId(string fullName)
        {
            return new AssemblyId(fullName);
        }

        internal string FullName
        {
            get { return base._name; }
        }

        public bool Equals(AssemblyId other)
        {
            return base.Equals(other);
        }
    }
}
