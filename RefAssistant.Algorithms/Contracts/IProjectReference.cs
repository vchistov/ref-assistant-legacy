using System;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface IProjectReference
    {
        string Name { get; }

        IAssembly Assembly { get; }

#warning TODO: possible useless
      
        //string Location { get; }

        //bool IsSpecificVersion { get; }
    }
}
