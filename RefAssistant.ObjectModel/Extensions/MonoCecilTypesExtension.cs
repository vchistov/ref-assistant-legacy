//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Mono.Cecil;

namespace Lardite.RefAssistant.Extensions
{
    /// <summary>
    /// Extentions for types of the Mono.Cecil library.
    /// </summary>
    static class MonoCecilTypesExtension
    {
        #region Fields

        private readonly static PublicKeyTokenConverter _publicKeyTokenConverter = new PublicKeyTokenConverter();

        #endregion // Fields

        #region Public methods

        /// <summary>
        /// Gets the assembly-qualified name of the <see cref="Mono.Cecil.TypeReference"/>, which includes the name of the assembly from which the <see cref="Mono.Cecil.MemberReference"/> was loaded.
        /// </summary>
        /// <param name="mr">The type reference derived object.</param>
        /// <returns>Returns string value.</returns>
        public static string AssemblyQualifiedName(this TypeReference mr)
        {
            return string.Format("{0}, {1}", mr.FullName, mr.Scope.AssemblyQualifiedName());
        }

        /// <summary>
        /// Gets the assembly-qualified name of the <see cref="Mono.Cecil.IMetadataScope"/>, which includes the name of the assembly, version number, culture and public key token.
        /// </summary>
        /// <param name="scope">The metadata scope.</param>
        /// <returns>Returns full name.</returns>
        public static string AssemblyQualifiedName(this IMetadataScope scope)
        {            
            var assemblyNameRef = GetAssemblyNameRefByMetadataScope(scope);
            return (assemblyNameRef != null) ? assemblyNameRef.FullName : scope.Name;
        }

        /// <summary>
        /// Gets scope public key token.
        /// </summary>
        /// <param name="scope">The metadata scope.</param>
        /// <returns>Returns PublicKeyToken string is exists otherwise empty string.</returns>
        public static string GetScopePublicKeyToken(this IMetadataScope scope)
        {
            var assemblyNameRef = GetAssemblyNameRefByMetadataScope(scope);
            return (assemblyNameRef != null) 
                ? _publicKeyTokenConverter.ConvertFrom(assemblyNameRef.PublicKeyToken) 
                : null;
        }

        /// <summary>
        /// Gets scope name. For some assemblies scope name can content file extension (.dll or .exe), this method removes it.
        /// </summary>
        /// <param name="scope">The metadata scope.</param>
        /// <returns>Returns metadata scope name.</returns>
        public static string GetAssemblyName(this IMetadataScope scope)
        {
            var assemblyNameRef = GetAssemblyNameRefByMetadataScope(scope);
            return (assemblyNameRef != null) ? assemblyNameRef.Name : scope.Name;
        }

        /// <summary>
        /// Resolve <see cref="TypeReference"/>.
        /// </summary>
        /// <param name="typeRef">The type reference.</param>
        /// <param name="forwardedFrom">The module from where type is forwarded.</param>
        /// <returns>Returns <see cref="TypeDefinition"/> of specified type reference; otherwise null.</returns>
        public static TypeDefinition Resolve(this TypeReference typeRef, out IMetadataScope forwardedFrom)
        {
            forwardedFrom = null;

            var typeDef = typeRef.Resolve();
            if (typeDef != null)
            {
                // not forwarded type
                return typeDef;
            }

            var assemblyResolver = GetAssemblyResolver(typeRef.Module);
            if (assemblyResolver == null)
            {
                return null;
            }

            var projectAssembly = assemblyResolver.Resolve(GetAssemblyNameRefByMetadataScope(typeRef.Scope));
            if (projectAssembly != null)
            {
                // need to get generic type without presentation of T, 
                // e.g. need "System.Collection.ObjectModel.ObservableCollection`1"
                // instead of "System.Collection.ObjectModel.ObservableCollection`1<System.Int32>"
                string typeFullName = typeRef.IsGenericInstance
                    ? string.Format("{0}.{1}", typeRef.Namespace, typeRef.Name)
                    : typeRef.FullName;

                var forwardedType = projectAssembly.Modules.SelectMany(m => m.ExportedTypes)
                    .Where(t => t.IsForwarder && t.FullName.Equals(typeFullName, StringComparison.InvariantCultureIgnoreCase))
                    .SingleOrDefault();

                // type is forwarded
                if (forwardedType != null)
                {
                    // initialize an output param
                    forwardedFrom = projectAssembly.MainModule;

                    var forwardedToAssembly = assemblyResolver
                        .Resolve(GetAssemblyNameRefByMetadataScope(forwardedType.Scope));

                    // search type
                    var type = forwardedToAssembly.Modules.SelectMany(m => m.Types)
                        .Where(t => t.FullName.Equals(typeFullName, StringComparison.InvariantCultureIgnoreCase))
                        .SingleOrDefault();

                    // if type was not found try to search nested type.
                    return (type != null) 
                        ? type 
                        : forwardedToAssembly.Modules.SelectMany(m => m.Types).SelectMany(t => t.NestedTypes)
                        .Where(t => t.FullName.Equals(typeFullName, StringComparison.InvariantCultureIgnoreCase))
                        .SingleOrDefault();
                }
            }

            return null;
        }

        /// <summary>
        /// Gets interfaces.
        /// </summary>
        /// <param name="td">Type definition.</param>
        /// <returns>Interfaces.</returns>
        public static IEnumerable<TypeReference> GetInterfaces(this TypeDefinition td)
        {
            return GetInterfaceHierarchy(td).Distinct(new TypeReferenceComparer());
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

        /// <summary>
        /// Get types definitions for modules collection.
        /// </summary>
        /// <param name="module">The modules definitions list.</param>
        /// <returns>Returns list of types definitions.</returns>
        public static IEnumerable<TypeDefinition> GetTypesDefinitions(this IEnumerable<ModuleDefinition> modules)
        {
            return modules.SelectMany(m => m.GetTypesDefinitions());
        }

        #endregion // Public methods

        #region Private methods

        /// <summary>
        /// Gets <see cref="AssemblyNameReference"/> of specified <see cref="IMetadataScope"/> scope.
        /// </summary>
        /// <param name="scope">The metadata scope.</param>
        /// <returns>Returns <see cref="AssemblyNameReference"/> object by value of IMetadataScope's  MetadataScopeType.</returns>
        private static AssemblyNameReference GetAssemblyNameRefByMetadataScope(IMetadataScope scope)
        {
            switch (scope.MetadataScopeType)
            {
                case MetadataScopeType.AssemblyNameReference:
                    return (AssemblyNameReference)scope;

                case MetadataScopeType.ModuleDefinition:
                    return ((ModuleDefinition)scope).Assembly.Name;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets <see cref="IAssemblyResolver"/> instance from <see cref="ModuleDefinition"/> by Reflection.
        /// </summary>
        /// <param name="module">The module definition.</param>
        /// <returns>Returns the assembly resolver extracted from module definition.</returns>
        private static IAssemblyResolver GetAssemblyResolver(ModuleDefinition module)
        {
            return (IAssemblyResolver)module.GetType()
                .GetField("AssemblyResolver", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField)
                .GetValue(module);
        }

        /// <summary>
        /// Gets interface hierarchy.
        /// </summary>
        /// <param name="td">Type definition.</param>
        /// <returns>Interface hierarchy.</returns>
        private static IEnumerable<TypeReference> GetInterfaceHierarchy(TypeDefinition td)
        {
            if (td == null)
            {
                yield break;
            }

            foreach (var i in td.Interfaces)
            {
                yield return i;
            }

            if (td.BaseType != null)
            {
                IMetadataScope forwardedFrom;
                foreach (var i in GetInterfaceHierarchy(td.BaseType.Resolve(out forwardedFrom)))
                {
                    yield return i;
                }
            }
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

        #endregion // Private methods
    }
}