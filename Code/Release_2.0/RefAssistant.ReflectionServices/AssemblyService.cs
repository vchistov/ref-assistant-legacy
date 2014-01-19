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
        private readonly IAssemblyContainer _container;

        internal AssemblyService(IAssemblyContainer container)
        {
            Contract.Requires(container != null);

            _container = container;
        }

        AssemblyInfo IAssemblyService.GetAssembly(string fileName)
        {
            ThrowUtils.ArgumentNullOrEmpty(() => fileName);

            var assemblyId = FileAssemblyIdProvider.GetId(fileName);
            return ((IAssemblyService)this).GetAssembly(assemblyId);
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
