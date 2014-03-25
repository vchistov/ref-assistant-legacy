using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface IProperty : IMember
    {
        IMemberType PropertyType { get; }

        IEnumerable<IMemberParameter> Parameters { get; }
    }
}
