using System.Diagnostics.Contracts;
using System.Linq;
using System.Collections.Generic;

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

        internal static IEnumerable<TypeDefinition> GetTypeDefinitions(this AssemblyDefinition @this)
        {
            Contract.Requires(@this != null);
            return @this.Modules.SelectMany(m => GetTypeDefinitions(m));
        }

        internal static IEnumerable<TypeReference> GetTypeReferences(this AssemblyDefinition @this)
        {
            Contract.Requires(@this != null);
            return @this.Modules.SelectMany(m => m.GetTypeReferences());
        }

        #region Helpers
        
        /// <summary>
        /// Gets types those defined in the module.
        /// </summary>
        /// <param name="this">The module definition.</param>
        /// <returns>Returns list of types definitions.</returns>
        private static IEnumerable<TypeDefinition> GetTypeDefinitions(ModuleDefinition moduleDef)
        {
            return moduleDef.Types.Union(moduleDef.Types.SelectMany(t => GetNestedTypes(t)));
        }

        private static IEnumerable<TypeDefinition> GetNestedTypes(TypeDefinition type)
        {
            if (type == null || !type.HasNestedTypes)
            {
                yield break;
            }

            foreach (var nestedType in type.NestedTypes)
            {
                yield return nestedType;
                foreach (var innerNestedType in GetNestedTypes(nestedType))
                {
                    yield return innerNestedType;
                }
            }
        }

        #endregion
    }
}
