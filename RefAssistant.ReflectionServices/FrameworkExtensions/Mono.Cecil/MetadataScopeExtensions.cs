using System;
using System.Diagnostics.Contracts;

namespace Mono.Cecil
{
    internal static class MetadataScopeExtensions
    {
        /// <summary>
        /// Gets <see cref="AssemblyNameReference"/> of specified <see cref="IMetadataScope"/> scope.
        /// </summary>
        /// <param name="scope">The metadata scope.</param>
        /// <returns>Returns <see cref="AssemblyNameReference"/> object by value of IMetadataScope's  MetadataScopeType.</returns>
        public static AssemblyNameReference GetAssemblyNameReference(this IMetadataScope @this)
        {
            Contract.Requires(@this != null);

            switch (@this.MetadataScopeType)
            {
                case MetadataScopeType.AssemblyNameReference:
                    return (AssemblyNameReference)@this;

                case MetadataScopeType.ModuleDefinition:
                    return ((ModuleDefinition)@this).Assembly.Name;

                default:
                    throw new NotSupportedException(string.Format("The metadata scope type '{0}' is not supported.", @this.MetadataScopeType));
            }
        }
    }
}
