using System;
using Mono.Cecil;
using System.Linq;
using System.Collections.Generic;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Readers
{
    internal sealed class AssemblyDefinitionReader : IAssemblyDefinitionReader
    {
        private readonly AssemblyDefinition _assemblyDefinition;

        internal AssemblyDefinitionReader(AssemblyDefinition assemblyDefinition)
        {
            ThrowUtils.ArgumentNull(() => assemblyDefinition);
            _assemblyDefinition = assemblyDefinition;
        }

        string IAssemblyDefinitionReader.GetName()
        {
            return _assemblyDefinition.Name.Name;
        }

        Version IAssemblyDefinitionReader.GetVersion()
        {
            return _assemblyDefinition.Name.Version;
        }

        string IAssemblyDefinitionReader.GetCulture()
        {
            return string.IsNullOrWhiteSpace(_assemblyDefinition.Name.Culture)
                ? "neutral"
                : _assemblyDefinition.Name.Culture;
        }

        byte[] IAssemblyDefinitionReader.GetPublicKeyToken()
        {
            return _assemblyDefinition.Name.HasPublicKey
                ? _assemblyDefinition.Name.PublicKeyToken
                : new byte[0];
        }

        string IAssemblyDefinitionReader.GetFullName()
        {
            return _assemblyDefinition.FullName;
        }

        IEnumerable<string> IAssemblyDefinitionReader.GetManifestAssemblies()
        {
            return _assemblyDefinition
                .Modules
                .SelectMany(m => m.AssemblyReferences)
                .Select(name => name.FullName);
        }
    }
}
