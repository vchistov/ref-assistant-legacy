using System;
using System.Collections.Generic;
using System.Linq;
using Lardite.RefAssistant.ReflectionServices.Data;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Containers;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles.Xaml
{
    internal sealed class XmlnsAssemblyKeeper
    {
        private readonly IAssemblyContainer _container;
        private readonly Lazy<IList<AssemblyDefinition>> _assemblies;
        private readonly GenericCache<string, IList<Tuple<AssemblyDefinition, string>>> _cache 
            = new GenericCache<string, IList<Tuple<AssemblyDefinition, string>>>();

        internal XmlnsAssemblyKeeper(IAssemblyContainer container, IEnumerable<AssemblyDefinition> assemblies)
        {
            _container = container;
            _assemblies = new Lazy<IList<AssemblyDefinition>>(
                () => assemblies.Where(asmDef => XmlnsDefinitionFeed.HasXmlnsDefinitions(asmDef)).ToList());
        }

        public IEnumerable<IGrouping<AssemblyDefinition, string>> GetClrNamespace(string xmlns)
        {
            var result = _cache.GetOrAdd(xmlns, ns => LookupAssemblies(ns).ToList());
            return result.GroupBy(e => e.Item1, e => e.Item2);
        }

        #region Helpers

        private IEnumerable<Tuple<AssemblyDefinition, string>> LookupAssemblies(string xmlns)
        {
            foreach(var asmDef in _assemblies.Value)
            {
                var xmlnsInfos = XmlnsDefinitionFeed.GetXmlnsDefinitions(asmDef)
                    .Where(info => string.Equals(info.XmlNamespace, xmlns, StringComparison.OrdinalIgnoreCase));

                foreach (var info in xmlnsInfos)
                {
                    var assemblyContainer = string.IsNullOrWhiteSpace(info.AssemblyName)
                        ? asmDef
                        : _container.Get(AssemblyId.GetId(info.AssemblyName));

                    yield return new Tuple<AssemblyDefinition, string>(assemblyContainer, info.ClrNamespace);
                }
            }
        }

        #endregion
    }
}
