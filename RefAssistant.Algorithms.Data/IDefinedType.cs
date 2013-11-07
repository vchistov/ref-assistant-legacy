using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface IDefinedType : IType, IEquatable<IDefinedType>
    {
        // TODO: fields, events, properties

        IEnumerable<IMethod> Methods { get; }
    }
}
