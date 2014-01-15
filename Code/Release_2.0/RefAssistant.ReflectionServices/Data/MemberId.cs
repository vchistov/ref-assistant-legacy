using System;
using System.Diagnostics.Contracts;

namespace Lardite.RefAssistant.ReflectionServices.Data
{
    [Serializable]
    public abstract class MemberId : BaseNamedId
    {
        private readonly TypeId _typeId;

        protected MemberId(string fullName, TypeId typeId)
            : base(fullName)
        {
            ThrowUtils.ArgumentNull(() => typeId);

            _typeId = typeId;
        }

        internal string FullName
        {
            get { return _name; }
        }

        internal TypeId TypeId
        {
            get { return _typeId; }
        }

        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
                return false;

            return this.TypeId.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ _typeId.GetHashCode();
        }
    }

    [Serializable]
    public sealed class MethodId : MemberId, IEquatable<MethodId>
    {
        private MethodId(string fullName, TypeId typeId)
            : base(fullName, typeId)
        {
        }

        public static MethodId GetId(string fullName, TypeId typeId)
        {
            return new MethodId(fullName, typeId);
        }

        public bool Equals(MethodId other)
        {
            return base.Equals(other);
        }
    }

    [Serializable]
    public sealed class FieldId : MemberId, IEquatable<FieldId>
    {
        private FieldId(string fullName, TypeId typeId)
            : base(fullName, typeId)
        {
        }

        public static FieldId GetId(string fullName, TypeId typeId)
        {
            return new FieldId(fullName, typeId);
        }

        public bool Equals(FieldId other)
        {
            return base.Equals(other);
        }
    }

    [Serializable]
    public sealed class PropertyId : MemberId, IEquatable<PropertyId>
    {
        private PropertyId(string fullName, TypeId typeId)
            : base(fullName, typeId)
        {
        }

        public static PropertyId GetId(string fullName, TypeId typeId)
        {
            return new PropertyId(fullName, typeId);
        }

        public bool Equals(PropertyId other)
        {
            return base.Equals(other);
        }
    }

    [Serializable]
    public sealed class EventId : MemberId, IEquatable<EventId>
    {
        private EventId(string fullName, TypeId typeId)
            : base(fullName, typeId)
        {
        }

        public static EventId GetId(string fullName, TypeId typeId)
        {
            return new EventId(fullName, typeId);
        }

        public bool Equals(EventId other)
        {
            return base.Equals(other);
        }
    }
}
