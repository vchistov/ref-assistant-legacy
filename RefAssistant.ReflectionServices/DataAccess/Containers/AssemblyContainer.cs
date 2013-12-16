using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Lardite.RefAssistant.ReflectionServices.Data.Assembly;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Loaders;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Readers;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Containers
{
    internal sealed class AssemblyContainer
    {
        private readonly AssemblyLoader _loader;
        private readonly AssemblyCache _cache = new AssemblyCache();

        public AssemblyContainer(AssemblyLoader loader)
        {
            Contract.Requires(loader != null);
            _loader = loader;
        }

        public IAssemblyDefinitionReader Get(AssemblyId assemblyId)
        {
            return _cache.Get(
                assemblyId,
                (id) => new AssemblyDefinitionReader(_loader.Load(id)));
        }

        private class AssemblyCache
        {
            private readonly IDictionary<AssemblyId, IAssemblyDefinitionReader> _cache = new Dictionary<AssemblyId, IAssemblyDefinitionReader>();

            public IAssemblyDefinitionReader Get(AssemblyId assemblyId, Func<AssemblyId, IAssemblyDefinitionReader> valueResolver)
            {
                IAssemblyDefinitionReader assemblyDefinition = null;
                if (!_cache.TryGetValue(assemblyId, out assemblyDefinition))
                {
                    assemblyDefinition = valueResolver(assemblyId);
                    _cache.Add(assemblyId, assemblyDefinition);
                }
                return assemblyDefinition;
            }
        }
    }
}
