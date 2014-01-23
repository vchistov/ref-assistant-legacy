using System;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface IMember : ICustomAttributeProvider
    {
        string Name { get; }
    }
}
