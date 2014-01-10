using System.Collections.Generic;
using System.Linq;

namespace Mono.Cecil
{
    internal static class TypeDefinitionExtensions
    {
        /// <summary>
        /// Get types definitions for modules collection.
        /// </summary>
        /// <param name="module">The modules definitions list.</param>
        /// <returns>Returns list of types definitions.</returns>
        public static IEnumerable<TypeDefinition> GetTypesDefinitions(this IEnumerable<ModuleDefinition> modules)
        {
            return modules.SelectMany(m => m.GetTypesDefinitions());
        }

        /// <summary>
        /// Get types definitions for module.
        /// </summary>
        /// <param name="module">The module definition.</param>
        /// <returns>Returns list of types definitions.</returns>
        public static IEnumerable<TypeDefinition> GetTypesDefinitions(this ModuleDefinition module)
        {
            return module.Types.Union(module.Types.SelectMany(t => GetNestedTypes(t)));
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
