using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

using Lardite.RefAssistant.ReflectionServices.Data;
using Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles.Baml;
using Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles.Xaml;
using Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles.Xaml.Resolvers;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Containers;

using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles
{
    internal sealed class EmbeddedCodeFilesGrubber : IEmbeddedCodeFilesGrubber
    {
        private readonly IAssemblyContainer _container;
        private readonly Lazy<AssemblyDefinition[]> _referenceAssemblies;

        private IXamlTypeResolver[] _xamlTypeResolvers;

        internal EmbeddedCodeFilesGrubber(IAssemblyContainer container, IEnumerable<string> referenceAssemblies)
        {
            Contract.Requires(container != null);
            Contract.Requires(referenceAssemblies != null);

            _container = container;
            _referenceAssemblies = new Lazy<AssemblyDefinition[]>(
                () => referenceAssemblies
                    .Select(fileName => _container.Get(FileAssemblyIdProvider.GetId(fileName)))
                    .ToArray());
        }

        public IEnumerable<TypeId> GetReferencedTypes(AssemblyId assemblyId)
        {
            var assemblyDef = _container.Get(assemblyId);
            Contract.Assert(assemblyDef != null);

            _xamlTypeResolvers = CreateTypeResolvers(assemblyDef).ToArray();

            return new EmbeddedResourceReader(assemblyDef)
                .SelectMany(HandleResource)
                .Distinct();
        }

        #region Helpers

        private IEnumerable<TypeId> HandleResource(KeyValuePair<string, Stream> resourceEntry)
        {
            using (Stream stream = resourceEntry.Value)
            {
                return GetHandler(resourceEntry)
                    .ResolveReferencedTypes()
                    .ToList();
            }
        }

        private ICodeFileHandler GetHandler(KeyValuePair<string, Stream> resourceEntry)
        {
            var fileName = resourceEntry.Key;
            if (fileName.EndsWith(".baml", StringComparison.OrdinalIgnoreCase))
            {
                return new BamlCodeFileHandler(resourceEntry.Value);
            }
            else if (fileName.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            {
                return new XamlCodeFileHandler(resourceEntry.Value, _xamlTypeResolvers);
            }

            return NullCodeFileHandler.Instance;
        }

        private IEnumerable<IXamlTypeResolver> CreateTypeResolvers(AssemblyDefinition assemblyDef)
        {
            yield return new ClrnsXamlTypeResolver(assemblyDef);

            yield return new XmlnsXamlTypeResolver(
                _container,
                _referenceAssemblies.Value.Union(new[] { assemblyDef }),
                TypeIdProvider.Instance);
        }

        private class NullCodeFileHandler : Singleton<NullCodeFileHandler>, ICodeFileHandler
        {
            public IEnumerable<TypeId> ResolveReferencedTypes()
            {
                yield break;
            }
        }

        #endregion
    }
}
