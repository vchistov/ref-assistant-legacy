using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Lardite.RefAssistant.ReflectionServices.Data
{
    [Serializable]
    public sealed class PublicKeyToken : IEquatable<PublicKeyToken>
    {
        private readonly byte[] _sourceToken;

        [NonSerialized]
        private readonly Lazy<ReadOnlyCollection<byte>> _token;

        internal PublicKeyToken(byte[] token)
        {
            _sourceToken = token ?? new byte[0];
            _token = new Lazy<ReadOnlyCollection<byte>>(() => Array.AsReadOnly(_sourceToken));
        }

        public ReadOnlyCollection<byte> Token
        {
            get { return _token.Value; }
        }

        bool IEquatable<PublicKeyToken>.Equals(PublicKeyToken other)
        {
            if (other == null)
            {
                return false;
            }

            return ((IStructuralEquatable)_sourceToken).Equals(other.Token.ToArray(), StructuralComparisons.StructuralEqualityComparer);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return ((IEquatable<PublicKeyToken>)this).Equals(obj as PublicKeyToken);
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable)_sourceToken).GetHashCode(StructuralComparisons.StructuralEqualityComparer);
        }

        public override string ToString()
        {
            string publicKeyToken = _sourceToken.Length > 0 ? string.Empty : "null";

            Array.ForEach(_sourceToken, t => { publicKeyToken += t.ToString("x2", CultureInfo.InvariantCulture); });
            return publicKeyToken;
        }
    }
}
