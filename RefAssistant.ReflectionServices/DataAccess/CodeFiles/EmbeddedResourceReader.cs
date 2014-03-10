using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Resources;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles
{
    using ResourceEntry = KeyValuePair<string, Stream>;

    internal sealed class EmbeddedResourceReader : IEnumerable<ResourceEntry>
    {
        private readonly AssemblyDefinition _assemblyDef;

        public EmbeddedResourceReader(AssemblyDefinition assemblyDef)
        {
            Contract.Requires(assemblyDef != null);
            _assemblyDef = assemblyDef;
        }

        public IEnumerator<ResourceEntry> GetEnumerator()
        {
            return this.GetEmbeddedResources()
                .SelectMany(ReadResource)
                .GetEnumerator();        
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ResourceEntry>)this).GetEnumerator();
        }

        #region Helpers

        private IEnumerable<EmbeddedResource> GetEmbeddedResources()
        {
            return _assemblyDef.Modules
                   .SelectMany(m => m.Resources)
                   .Where(r => r is EmbeddedResource)
                   .Cast<EmbeddedResource>();
        }

        private IEnumerable<ResourceEntry> ReadResource(EmbeddedResource resource)
        {
            if (resource.Name.EndsWith(".g.resources", StringComparison.Ordinal))
            {
                using (var reader = new ResourceReader(resource.GetResourceStream()))
                {
                    foreach (DictionaryEntry entry in reader)
                    {
                        yield return new ResourceEntry((string)entry.Key, (Stream)entry.Value);
                    }
                }
            }

            yield return new ResourceEntry(resource.Name, resource.GetResourceStream());
        }

        #endregion
    }
}
