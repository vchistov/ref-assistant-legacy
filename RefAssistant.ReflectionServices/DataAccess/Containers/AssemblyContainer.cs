using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Lardite.RefAssistant.ReflectionServices.Data;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Readers;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Containers
{
    using AssemblyCache = Lardite.RefAssistant.GenericCache<AssemblyId, AssemblyDefinition>;

    internal sealed class AssemblyContainer : IAssemblyContainer
    {
        private readonly IAssemblyResolver _assemblyResolver;
        private readonly AssemblyCache _cache = new AssemblyCache();

        public AssemblyContainer(IAssemblyResolver assemblyResolver)
        {
            Contract.Requires(assemblyResolver != null);
            _assemblyResolver = assemblyResolver;
        }

        AssemblyDefinition IAssemblyContainer.Get(AssemblyId assemblyId)
        {
            return _cache.Get(assemblyId, (id) => LoadAssembly(id));
        }

        private AssemblyDefinition LoadAssembly(AssemblyId assemblyId)
        {
            Contract.Requires(assemblyId != null);
            Contract.Ensures(Contract.Result<AssemblyDefinition>() != null);
            Contract.Assert(_assemblyResolver != null);

            var assemblyName = AssemblyNameReference.Parse(assemblyId.FullName);
            return _assemblyResolver.Resolve(assemblyName);
        }
    }
}
