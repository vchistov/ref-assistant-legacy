using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface IProject
    {
        string Name { get; }

        IAssembly Assembly { get; }

        IEnumerable<IProjectReference> ProjectRefs { get; }
    }
}
