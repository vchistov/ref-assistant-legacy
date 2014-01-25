using System.Collections.Generic;

namespace Lardite.RefAssistant.Collections
{
    public static class HashSetExtensions
    {
        public static IReadOnlyHashSet<T> AsReadOnly<T>(this ISet<T> source)
        {
            return new ReadOnlyHashSet<T>(source);
        }

        public static ISet<T> UnionWithFluent<T>(this ISet<T> source, IEnumerable<T> other)
        {
            source.UnionWith(other);
            return source;
        }
    }
}
