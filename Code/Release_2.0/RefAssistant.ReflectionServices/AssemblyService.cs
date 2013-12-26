using System;
using System.Collections.Generic;
using System.Linq;
using Lardite.RefAssistant.ReflectionServices.Data;
using Lardite.RefAssistant.ReflectionServices.DataAccess;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Containers;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Readers;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices
{
    internal sealed class AssemblyService : IAssemblyService
    {
        private readonly Lazy<AssemblyId> _projectAssemblyId;
        private readonly IAssemblyContainer _container;

        internal AssemblyService(IAssemblyIdProvider projectAssemblyIdProvider, IAssemblyContainer container)
        {
            ThrowUtils.ArgumentNull(() => projectAssemblyIdProvider);
            ThrowUtils.ArgumentNull(() => container);

            _projectAssemblyId = new Lazy<AssemblyId>(projectAssemblyIdProvider.GetId);
            _container = container;
        }

        AssemblyInfo IAssemblyService.GetProjectAssembly()
        {
            var reader = CreateReader(_container.Get(_projectAssemblyId.Value));
            return new AssemblyInfo(reader);
        }

        AssemblyInfo IAssemblyService.GetAssembly(AssemblyId assemblyId)
        {
            ThrowUtils.ArgumentNull(() => assemblyId);

            var reader = CreateReader(_container.Get(assemblyId));
            return new AssemblyInfo(reader);
        }

        IEnumerable<AssemblyId> IAssemblyService.GetManifestAssemblies(AssemblyId assemblyId)
        {
            ThrowUtils.ArgumentNull(() => assemblyId);

            var reader = CreateReader(_container.Get(assemblyId));
            return reader
                .GetManifestAssemblies()
                .Select(a => AssemblyId.GetId(a));
        }

        private IAssemblyDefinitionReader CreateReader(AssemblyDefinition assemblyDefinition)
        {
            return new AssemblyDefinitionReader(assemblyDefinition);
        }
    }
}
