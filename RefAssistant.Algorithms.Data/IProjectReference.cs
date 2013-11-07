using System;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface IProjectReference : IEquatable<IProjectReference>
    {
        string Name { get; }

        IAssembly Assembly { get; }

#warning TODO: possible useless
      
        //string Location { get; }

        //bool IsSpecificVersion { get; }
    }
}
