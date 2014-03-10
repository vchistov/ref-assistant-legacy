using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace System.Xaml
{
    internal static class XamlTypeExtensions
    {
        public static string GetXmlNamespace(this XamlType @this)
        {
            Contract.Requires(@this != null);
            return IsXmlNamespace(@this) ? @this.PreferredXamlNamespace : null;
        }

        public static string GetClrNamespace(this XamlType @this)
        {
            Contract.Requires(@this != null);

            if (IsClrNamespace(@this))
            {
                var regex = new Regex(@"clr-namespace:\s*(?<namespace>[^;]+)", RegexOptions.IgnoreCase);
                var match = regex.Match(@this.PreferredXamlNamespace);
                Contract.Assert(match.Success);

                return match.Groups["namespace"].Value;
            }

            return null;
        }

        public static string GetClrAssemblyName(this XamlType @this)
        {
            Contract.Requires(@this != null);

            if (IsClrNamespace(@this))
            {
                var regex = new Regex(@"assembly=\s*(?<assembly>[^,;]+)", RegexOptions.IgnoreCase);
                var match = regex.Match(@this.PreferredXamlNamespace);
                
                return match.Success
                    ? match.Groups["assembly"].Value
                    : null;
            }

            return null;
        }

        public static string GetFullName(this XamlType @this, string @namespace = null)
        {
            Contract.Requires(@this != null);

            string ns = @namespace ?? @this.GetClrNamespace();

            // the type in xaml cannot be nested, so we don't need to transform the name to cecil naming format.
            return string.IsNullOrWhiteSpace(ns)
                ? @this.Name
                : string.Format("{0}.{1}", ns, @this.Name);
        }

        /// <summary>
        /// Indicates if namespace is in XML fashion, not CLR.
        /// </summary>
        public static bool IsXmlNamespace(this XamlType @this)
        {
            Contract.Requires(@this != null);

            return @this.PreferredXamlNamespace.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || @this.PreferredXamlNamespace.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Indicates if namespace is in CLR fashion, not XML.
        /// </summary>
        public static bool IsClrNamespace(this XamlType @this)
        {
            Contract.Requires(@this != null);

            return @this.PreferredXamlNamespace.StartsWith("clr-namespace:", StringComparison.OrdinalIgnoreCase);
        }
    }
}
