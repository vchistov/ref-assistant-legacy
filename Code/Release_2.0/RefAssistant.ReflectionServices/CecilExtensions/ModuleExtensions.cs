using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;

namespace Mono.Cecil
{
    internal static class ModuleExtensions
    {
        /// <summary>
        /// Gets types those defined in the modules.
        /// </summary>
        /// <param name="this">The modules definitions list.</param>
        /// <returns>Returns list of types definitions.</returns>
        public static IEnumerable<TypeDefinition> GetTypeDefinitions(this IEnumerable<ModuleDefinition> @this)
        {
            Contract.Requires(@this != null);
            return @this.SelectMany(m => m.GetTypeDefinitions());
        }

        /// <summary>
        /// Gets types those defined in the module.
        /// </summary>
        /// <param name="this">The module definition.</param>
        /// <returns>Returns list of types definitions.</returns>
        public static IEnumerable<TypeDefinition> GetTypeDefinitions(this ModuleDefinition @this)
        {
            Contract.Requires(@this != null);
            return @this.Types.Union(@this.Types.SelectMany(t => GetNestedTypes(t)));
        }

        /// <summary>
        /// Gets types those referenced by the modules.
        /// </summary>
        /// <param name="this">The modules definitions list.</param>
        /// <returns>Returns list of type references.</returns>
        public static IEnumerable<TypeReference> GetTypeReferences(this IEnumerable<ModuleDefinition> @this)
        {
            Contract.Requires(@this != null);
            return @this.SelectMany(m => m.GetTypeReferences());
        }

        #region Helpers
        
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
