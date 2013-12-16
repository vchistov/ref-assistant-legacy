using System.Diagnostics.Contracts;
using Lardite.RefAssistant.ReflectionServices.Data.Assembly;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess
{
    internal sealed class ProjectAssemblyIdProvider : IAssemblyIdProvider
    {
        private readonly string _file;

        public ProjectAssemblyIdProvider(string file)
        {
            ThrowUtils.ArgumentNullOrEmpty(() => file);
            _file = file;
        }

        public AssemblyId GetId()
        {
            Contract.Ensures(Contract.Result<AssemblyId>() != null);

            return AssemblyId.GetId(GetFullName());
        }

        private string GetFullName()
        {
            var assembly = ModuleDefinition.ReadModule(_file).Assembly;
            return assembly.FullName;
        }
    }
}
