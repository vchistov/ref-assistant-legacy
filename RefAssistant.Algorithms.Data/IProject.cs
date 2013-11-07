using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface IProject : IEquatable<IProject>
    {
        string Name { get; }

        IAssembly Assembly { get; }

        IEnumerable<IProjectReference> ProjectRefs { get; }
    }
}
