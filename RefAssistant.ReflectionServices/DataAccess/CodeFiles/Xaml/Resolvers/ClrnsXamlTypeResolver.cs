using System.Diagnostics.Contracts;
using System.Xaml;
using Lardite.RefAssistant.ReflectionServices.Data;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles.Xaml.Resolvers
{
    internal sealed class ClrnsXamlTypeResolver : IXamlTypeResolver
    {
        private readonly AssemblyDefinition _hostAssembly;

        internal ClrnsXamlTypeResolver(AssemblyDefinition hostAssembly)
        {
            Contract.Requires(hostAssembly != null);

            _hostAssembly = hostAssembly;
        }

        public TypeId Resolve(XamlType xamlType)
        {
            if (!xamlType.IsClrNamespace())
            {
                return null;
            }

            string assemblyName = xamlType.GetClrAssemblyName() ?? GetHostAssemblyName();

            Contract.Assert(!string.IsNullOrWhiteSpace(assemblyName));

            var assemblyId = AssemblyId.GetId(assemblyName);
            var typeId = TypeId.GetId(xamlType.GetFullName(), assemblyId);

            return typeId;
        }

        private string GetHostAssemblyName()
        {
            return _hostAssembly.FullName;
        }
    }
}
