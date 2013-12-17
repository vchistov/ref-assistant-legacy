using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using System.Diagnostics.Contracts;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Readers
{
    internal sealed class AssemblyDefinitionReader : IAssemblyDefinitionReader
    {
        private readonly AssemblyDefinition _assemblyDefinition;

        internal AssemblyDefinitionReader(AssemblyDefinition assemblyDefinition)
        {
            Contract.Requires(assemblyDefinition != null);

            _assemblyDefinition = assemblyDefinition;
        }

        string IAssemblyDefinitionReader.GetName()
        {
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return _assemblyDefinition.Name.Name;
        }

        Version IAssemblyDefinitionReader.GetVersion()
        {
            Contract.Ensures(Contract.Result<Version>() != null);

            return _assemblyDefinition.Name.Version;
        }

        string IAssemblyDefinitionReader.GetCulture()
        {
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return string.IsNullOrWhiteSpace(_assemblyDefinition.Name.Culture)
                ? "neutral"
                : _assemblyDefinition.Name.Culture;
        }

        byte[] IAssemblyDefinitionReader.GetPublicKeyToken()
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return _assemblyDefinition.Name.HasPublicKey
                ? _assemblyDefinition.Name.PublicKeyToken
                : new byte[0];
        }

        string IAssemblyDefinitionReader.GetFullName()
        {
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return _assemblyDefinition.FullName;
        }

        IEnumerable<string> IAssemblyDefinitionReader.GetManifestAssemblies()
        {
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            return _assemblyDefinition
                .Modules
                .SelectMany(m => m.AssemblyReferences)
                .Select(name => name.FullName);
        }
    }
}
