using System;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface ITypeImport : ITypeDefinition, IEquatable<ITypeImport>
    {
        IAssembly ImportedFrom { get; }
    }
}
