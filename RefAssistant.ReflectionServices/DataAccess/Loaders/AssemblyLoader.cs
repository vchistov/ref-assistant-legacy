using Lardite.RefAssistant.ReflectionServices.Data.Assembly;
using Mono.Cecil;
using System.Diagnostics.Contracts;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Loaders
{
    internal sealed class AssemblyLoader
    {
        private readonly IAssemblyResolver _assemblyResolver;

        public AssemblyLoader(IAssemblyResolver assemblyResolver)
        {
            ThrowUtils.ArgumentNull(() => assemblyResolver);
            _assemblyResolver = assemblyResolver;
        }

        public AssemblyDefinition Load(AssemblyId assemblyId)
        {
            ThrowUtils.ArgumentNull(() => assemblyId);

            var assemblyName = AssemblyNameReference.Parse(assemblyId.FullName);
            return _assemblyResolver.Resolve(assemblyName, GetReaderParameters());
        }

        private ReaderParameters GetReaderParameters()
        {
            Contract.Requires(_assemblyResolver != null);

            return new ReaderParameters
            {
                AssemblyResolver = _assemblyResolver,
                ReadingMode = ReadingMode.Deferred
            };
        }
    }
}
