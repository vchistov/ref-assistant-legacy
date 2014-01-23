using System;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface ITypeImport : ITypeDefinition
    {
        IAssembly ImportedFrom { get; }
    }
}
