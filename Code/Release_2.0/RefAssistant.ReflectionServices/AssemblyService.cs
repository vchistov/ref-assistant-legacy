using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            Contract.Requires(projectAssemblyIdProvider != null);
            Contract.Requires(container != null);

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
            return reader.GetManifestAssemblies();
        }

        IEnumerable<TypeId> IAssemblyService.GetDefinedTypes(AssemblyId assemblyId)
        {
            ThrowUtils.ArgumentNull(() => assemblyId);

            var reader = CreateReader(_container.Get(assemblyId));
            return reader.GetTypeDefinitions();
        }

        IEnumerable<TypeId> IAssemblyService.GetReferencedTypes(AssemblyId assemblyId)
        {
            ThrowUtils.ArgumentNull(() => assemblyId);

            var reader = CreateReader(_container.Get(assemblyId));
            return reader.GetTypeReferences();
        }

        #region Helpers
        
        private IAssemblyDefinitionReader CreateReader(AssemblyDefinition assemblyDef)
        {
            return new AssemblyDefinitionReader(assemblyDef, TypeIdProvider.Instance);
        }

        #endregion
    }
}
