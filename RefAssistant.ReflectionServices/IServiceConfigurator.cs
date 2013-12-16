using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lardite.RefAssistant.ReflectionServices
{
    public interface IServiceConfigurator
    {
        IAssemblyService AssemblyService { get; }
    }
}
