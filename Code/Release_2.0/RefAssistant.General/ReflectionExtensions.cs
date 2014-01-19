using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lardite.RefAssistant
{
    public static class ReflectionExtensions
    {
        public static T GetCustomAttribute<T>(this ICustomAttributeProvider @this, bool inherit = false)
            where T : Attribute
        {
            if (@this.IsDefined(typeof(T), inherit))
            {
                return (T)@this.GetCustomAttributes(typeof(T), inherit).Single();
            }

            return default(T);
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeProvider @this, bool inherit = false)
            where T : Attribute
        {
            if (@this.IsDefined(typeof(T), inherit))
            {
                return @this.GetCustomAttributes(typeof(T), inherit).Cast<T>();
            }

            return Enumerable.Empty<T>();
        }
    }
}
