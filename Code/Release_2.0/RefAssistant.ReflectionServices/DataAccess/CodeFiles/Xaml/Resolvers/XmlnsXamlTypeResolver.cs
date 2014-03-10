using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xaml;
using Lardite.RefAssistant.ReflectionServices.Data;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Containers;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles.Xaml.Resolvers
{
    internal sealed class XmlnsXamlTypeResolver : IXamlTypeResolver
    {
        private readonly AssemblyDefinition[] _referenceAssemblies;
        private readonly ITypeIdProvider _typeIdProvider;
        private readonly XmlnsAssemblyKeeper _xmlnsAssemblyKeeper;

        internal XmlnsXamlTypeResolver(IAssemblyContainer container, IEnumerable<AssemblyDefinition> referenceAssemblies, ITypeIdProvider typeIdProvider)
        {
            Contract.Requires(container != null);
            Contract.Requires(referenceAssemblies != null);
            Contract.Requires(typeIdProvider != null);

            _referenceAssemblies = referenceAssemblies.ToArray();
            _typeIdProvider = typeIdProvider;
            _xmlnsAssemblyKeeper = new XmlnsAssemblyKeeper(container, referenceAssemblies);
        }

        public TypeId Resolve(XamlType xamlType)
        {
            if (!xamlType.IsXmlNamespace())
            {
                return null;
            }

            var xmlns = xamlType.GetXmlNamespace();

            foreach (var namespaces in _xmlnsAssemblyKeeper.GetClrNamespace(xmlns))
            {
                var assemblyDef = namespaces.Key;

                foreach (var ns in namespaces)
                {
                    var typeDef = assemblyDef.GetType(xamlType.GetFullName(ns));
                    if (typeDef != null)
                    {
                        return _typeIdProvider.GetId(typeDef);
                    }
                }
            }

            return null;
        }
    }
}
