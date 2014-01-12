using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Mono.Cecil
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Gets interfaces.
        /// </summary>
        /// <param name="this">Type definition.</param>
        /// <returns>Interfaces.</returns>
        public static IEnumerable<TypeReference> GetInterfaces(this TypeDefinition @this)
        {
            Contract.Requires(@this != null);
            return GetInterfacesFlat(@this);
        }

        /// <summary>
        /// Resolve <see cref="TypeReference"/>.
        /// </summary>
        /// <param name="this">The type reference.</param>
        /// <param name="forwardedFrom">The module from where type is forwarded.</param>
        /// <returns>Returns <see cref="TypeDefinition"/> of specified type reference; otherwise null.</returns>
        public static TypeDefinition Resolve(this TypeReference @this, out IMetadataScope forwardedFrom)
        {
            Contract.Requires(@this != null);

            // TODO: need to refactor this method

            forwardedFrom = null;

            var typeDef = @this.Resolve();
            if (typeDef != null && AreScopesEqual(typeDef.Scope, @this.Scope))
            {
                // not forwarded type
                return typeDef;
            }

            var assemblyResolver = @this.Module.AssemblyResolver;
            if (assemblyResolver == null)
            {
                return null;
            }

            var projectAssembly = assemblyResolver.Resolve(@this.Scope.GetAssemblyNameReference());
            if (projectAssembly != null)
            {
                // need to get generic type without presentation of T, 
                // e.g. need "System.Collection.ObjectModel.ObservableCollection`1"
                // instead of "System.Collection.ObjectModel.ObservableCollection`1<System.Int32>"
                string typeFullName = @this.IsGenericInstance
                    ? string.Format("{0}.{1}", @this.Namespace, @this.Name)
                    : @this.FullName;

                var forwardedType = projectAssembly.Modules.SelectMany(m => m.ExportedTypes)
                    .Where(t => t.IsForwarder && t.FullName.Equals(typeFullName, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();

                // type is forwarded
                if (forwardedType != null)
                {
                    // initialize an output param
                    forwardedFrom = projectAssembly.MainModule;

                    var forwardedToAssembly = assemblyResolver
                        .Resolve(forwardedType.Scope.GetAssemblyNameReference());

                    // search type
                    var type = forwardedToAssembly.Modules.SelectMany(m => m.Types)
                        .Where(t => t.FullName.Equals(typeFullName, StringComparison.OrdinalIgnoreCase))
                        .SingleOrDefault();

                    // if type was not found try to search nested type.
                    return (type != null)
                        ? type
                        : forwardedToAssembly.Modules.SelectMany(m => m.Types).SelectMany(t => t.NestedTypes)
                        .Where(t => t.FullName.Equals(typeFullName, StringComparison.OrdinalIgnoreCase))
                        .SingleOrDefault();
                }
            }

            return null;
        }

        #region Helpers

        private static IEnumerable<TypeReference> GetInterfacesFlat(TypeDefinition typeDef)
        {
            if (typeDef == null)
            {
                yield break;
            }

            foreach (var i in typeDef.Interfaces)
            {
                yield return i;
            }

            if (typeDef.BaseType != null)
            {
                foreach (var i in GetInterfacesFlat(typeDef.BaseType.Resolve()))
                {
                    yield return i;
                }
            }
        }

        private static bool AreScopesEqual(IMetadataScope scope1, IMetadataScope scope2)
        {
            if (ReferenceEquals(scope1, null) || ReferenceEquals(scope2, null))
                return false;

            if (ReferenceEquals(scope1, scope2))
                return true;

            var assemblyName1 = scope1.GetAssemblyNameReference();
            var assemblyName2 = scope2.GetAssemblyNameReference();

            if (ReferenceEquals(assemblyName1, null) && ReferenceEquals(assemblyName2, null))
            {
                return string.Equals(scope1.Name, scope2.Name, StringComparison.OrdinalIgnoreCase);
            }

            if (ReferenceEquals(assemblyName1, null) || ReferenceEquals(assemblyName2, null))
            {
                return false;
            }

            return AreAssembliesEqual(assemblyName1, assemblyName2, true);
        }

        private static bool AreAssembliesEqual(AssemblyNameReference assemblyName1, AssemblyNameReference assemblyName2, bool ignoreVersion)
        {
            // names comparing
            bool result = string.Equals(assemblyName1.Name, assemblyName2.Name, StringComparison.OrdinalIgnoreCase);

            // cultures comparing
            result &= GetAssemblyCulture(assemblyName1).Equals(GetAssemblyCulture(assemblyName2), StringComparison.OrdinalIgnoreCase);

            // public key tokens comparing
            result &= assemblyName1.PublicKeyToken != null
                && assemblyName2.PublicKeyToken != null
                && assemblyName1.PublicKeyToken.SequenceEqual(assemblyName2.PublicKeyToken);

            // versions comparing
            if (!ignoreVersion)
            {
                result &= assemblyName1.Version.Equals(assemblyName2.Version);
            }

            return result;
        }

        private static string GetAssemblyCulture(AssemblyNameReference assemblyName)
        {
            return string.IsNullOrWhiteSpace(assemblyName.Culture) ? "neutral" : assemblyName.Culture;
        }

        #endregion
    }
}
