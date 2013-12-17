using System.Diagnostics.Contracts;
using Lardite.RefAssistant.ReflectionServices.Data.Assembly;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Loaders
{
    internal sealed class AssemblyLoader
    {
        private readonly IAssemblyResolver _assemblyResolver;

        public AssemblyLoader(IAssemblyResolver assemblyResolver)
        {
            Contract.Requires(assemblyResolver != null);
            _assemblyResolver = assemblyResolver;
        }

        public AssemblyDefinition Load(AssemblyId assemblyId)
        {
            Contract.Requires(assemblyId != null);
            Contract.Ensures(Contract.Result<AssemblyDefinition>() != null);
            Contract.Assert(_assemblyResolver != null);

            var assemblyName = AssemblyNameReference.Parse(assemblyId.FullName);
            return _assemblyResolver.Resolve(assemblyName);
        }
    }
}
