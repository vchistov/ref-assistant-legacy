using System;
using System.Collections.Generic;
using System.Linq;
using Lardite.RefAssistant.ReflectionServices.Data.Assembly;
using Lardite.RefAssistant.ReflectionServices.DataAccess;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Containers;

namespace Lardite.RefAssistant.ReflectionServices
{
    internal class AssemblyService : IAssemblyService
    {
        private readonly Lazy<AssemblyId> _projectAssemblyId;
        private readonly AssemblyContainer _container;

        public AssemblyService(IAssemblyIdProvider projectAssemblyIdProvider, AssemblyContainer container)
        {
            ThrowUtils.ArgumentNull(() => projectAssemblyIdProvider);
            ThrowUtils.ArgumentNull(() => container);

            _projectAssemblyId = new Lazy<AssemblyId>(projectAssemblyIdProvider.GetId);
            _container = container;
        }

        AssemblyInfo IAssemblyService.GetProjectAssembly()
        {
            var reader = _container.Get(_projectAssemblyId.Value);
            return new AssemblyInfo(reader);
        }

        AssemblyInfo IAssemblyService.GetAssembly(AssemblyId assemblyId)
        {
            ThrowUtils.ArgumentNull(() => assemblyId);

            var reader = _container.Get(assemblyId);
            return new AssemblyInfo(reader);
        }

        IEnumerable<AssemblyId> IAssemblyService.GetManifestAssemblies(AssemblyId assemblyId)
        {
            ThrowUtils.ArgumentNull(() => assemblyId);

            var reader = _container.Get(assemblyId);
            return reader
                .GetManifestAssemblies()
                .Select(a => AssemblyId.GetId(a));
        }
    }
}
