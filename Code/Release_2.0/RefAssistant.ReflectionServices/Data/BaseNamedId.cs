using System;
using System.Diagnostics.Contracts;

namespace Lardite.RefAssistant.ReflectionServices.Data
{
    [Serializable]
    public abstract class BaseNamedId
    {
        protected readonly string _name;

        protected BaseNamedId(string name)
        {
            ThrowUtils.ArgumentNullOrEmpty(() => name);
            _name = name;
        }

        public static bool operator ==(BaseNamedId left, BaseNamedId right)
        {
            return object.Equals(left, right);
        }

        public static bool operator !=(BaseNamedId left, BaseNamedId right)
        {
            return !object.Equals(left, right);
        }

        public override bool Equals(object other)
        {
            return this.Equals(other as BaseNamedId);            
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public override string ToString()
        {
            return _name;
        }

        private bool Equals(BaseNamedId other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            return string.Equals(_name, other._name, StringComparison.Ordinal);
        }
    }
}
