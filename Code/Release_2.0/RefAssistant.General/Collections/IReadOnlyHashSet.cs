using System.Collections;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Collections
{
    public interface IReadOnlyHashSet<T> : IEnumerable<T>, IEnumerable
    {
        int Count { get; }
    }
}
