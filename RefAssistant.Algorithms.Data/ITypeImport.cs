using System;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface ITypeImport : IType, IEquatable<ITypeImport>
    {
        IAssembly ImportedFrom { get; }
    }
}
