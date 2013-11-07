using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Data
{
    public interface IAssembly : ICustomAttributeProvider, IEquatable<IAssembly>
    {
        string Name { get; }

        Version Version { get; }

        string Culture { get; }

        IPublicKeyToken PublicKeyToken { get; }

        IEnumerable<IAssembly> References { get; }

#warning TODO: possible useless
        //IEnumerable<ITypeInfo> Types { get; }
    }
}
