using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface IReferencedType : IType, IEquatable<IReferencedType>
    {
    }
}
