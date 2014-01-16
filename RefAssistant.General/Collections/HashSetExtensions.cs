using System.Collections.Generic;

namespace Lardite.RefAssistant.Collections
{
    public static class HashSetExtensions
    {
        public static IReadOnlyHashSet<T> AsReadOnly<T>(this ISet<T> source)
        {
            return new ReadOnlyHashSet<T>(source);
        }
    }
}
