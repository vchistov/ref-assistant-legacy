using System.Diagnostics.Contracts;
using System.Linq;

namespace Mono.Cecil
{
    internal static class AssemblyDefinitionExtensions
    {
        internal static TypeDefinition GetType(this AssemblyDefinition @this, string fullName)
        {
            Contract.Requires(@this != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(fullName));

            return @this.Modules
                .Select(m => m.GetType(fullName))
                .FirstOrDefault();
        }
    }
}
