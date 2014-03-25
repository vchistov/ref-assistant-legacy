using System.Collections.Generic;
using System.Linq;

namespace Lardite.RefAssistant
{
    public static class EnumerableExtensions
    {
        public static bool HasElements<T>(this IEnumerable<T> @this)
        {
            return @this == null
                ? false
                : @this.Any();
        }

        public static IEnumerable<T> AsNotNull<T>(this IEnumerable<T> @this)
        {
            return @this ?? Enumerable.Empty<T>();
        }
    }
}
