using System.Diagnostics.Contracts;
using Lardite.RefAssistant.ReflectionServices.Data;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess
{
    internal static class FileAssemblyIdProvider
    {
        public static AssemblyId GetId(string fileName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(fileName));
            Contract.Ensures(Contract.Result<AssemblyId>() != null);

            return AssemblyId.GetId(GetFullName(fileName));
        }

        private static string GetFullName(string fileName)
        {         
            var assembly = ModuleDefinition.ReadModule(fileName).Assembly;
            return assembly.FullName;
        }
    }
}
