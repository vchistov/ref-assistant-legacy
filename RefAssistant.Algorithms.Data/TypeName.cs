using System;
using System.Text;

namespace Lardite.RefAssistant.Algorithms.Data
{
    [Serializable]
    public sealed class TypeName : IEquatable<TypeName>
    {
        private readonly ITypeDefinition _typeInfo;

        public TypeName(ITypeDefinition typeInfo)
        {
            _typeInfo = typeInfo;
        }

        public string FullName
        {
            get;
            set;
        }

        public string AssemblyQualifiedName
        {
            get 
            {
                var sb = new StringBuilder(FullName);
                if (_typeInfo.Assembly != null)
                {
                    sb.Append(", ").Append(_typeInfo.Assembly.ToString());
                }
                return sb.ToString();
            }
        }

        bool IEquatable<TypeName>.Equals(TypeName other)
        {
            if (other == null)
                return false;

            return string.Equals(AssemblyQualifiedName, other.AssemblyQualifiedName, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
                return false;

            return (this as IEquatable<TypeName>).Equals(obj as TypeName);
        }

        public override string ToString()
        {
            return FullName;
        }

        public override int GetHashCode()
        {
            return new { AssemblyQualifiedName }.GetHashCode();
        }
    }
}
