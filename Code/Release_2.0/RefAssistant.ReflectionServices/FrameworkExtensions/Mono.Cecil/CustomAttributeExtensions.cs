using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Mono.Cecil
{
    internal static class CustomAttributeExtensions
    {
        private const string GuidAttributeName = "System.Runtime.InteropServices.GuidAttribute";
        private const string CompilerGeneratedAttributeName = "System.Runtime.CompilerServices.CompilerGeneratedAttribute";

        public static bool HasCustomAttribute(this ICustomAttributeProvider @this, string typeName)
        {
            Contract.Requires(@this != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(typeName));

            if (!@this.HasCustomAttributes)
            {
                return false;
            }

            return @this.CustomAttributes
                .Any(c => CustomAttributeFilter(c, typeName));
        }

        public static bool HasCustomAttribute(this ICustomAttributeProvider @this, TypeReference type)
        {
            Contract.Requires(type != null);
            return HasCustomAttribute(@this, type.FullName);
        }

        public static IEnumerable<CustomAttribute> GetCustomAttributes(this ICustomAttributeProvider @this, string typeName)
        {
            Contract.Requires(@this != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(typeName));

            if (!@this.HasCustomAttributes)
            {
                return Enumerable.Empty<CustomAttribute>();
            }

            return @this.CustomAttributes
                .Where(c => CustomAttributeFilter(c, typeName));
        }

        public static IEnumerable<CustomAttribute> GetCustomAttributes(this ICustomAttributeProvider @this, TypeReference type)
        {
            Contract.Requires(type != null);
            return GetCustomAttributes(@this, type.FullName);
        }

        public static Guid? GetGuid(this ICustomAttributeProvider @this)
        {
            var attribute = GetCustomAttributes(@this, GuidAttributeName).SingleOrDefault();
            if (attribute != null)
            {
                Contract.Assert(attribute.HasConstructorArguments);
                var arg = attribute.ConstructorArguments.Single();
                return Guid.Parse((string)arg.Value);
            }

            return null;
        }

        public static bool IsCompilerGenerated(this ICustomAttributeProvider @this)
        {
            return HasCustomAttribute(@this, CompilerGeneratedAttributeName);
        }

        #region Helpers

        private static bool CustomAttributeFilter(CustomAttribute customAttribute, string typeNameFilter, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return string.Equals(customAttribute.AttributeType.FullName, typeNameFilter, comparisonType);
        }

        #endregion
    }
}
