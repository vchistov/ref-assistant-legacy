using System;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface IMember : ICustomAttributeProvider, IEquatable<IMember>
    {
        string Name { get; }
    }
}
