using System;
using System.Collections.ObjectModel;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface IPublicKeyToken : IEquatable<IPublicKeyToken>
    {
        ReadOnlyCollection<byte> Token { get; }
    }
}
