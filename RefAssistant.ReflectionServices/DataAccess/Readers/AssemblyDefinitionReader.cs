using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.ReflectionServices.Data;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Readers
{
    internal sealed class AssemblyDefinitionReader : IAssemblyDefinitionReader
    {
        private readonly AssemblyDefinition _assemblyDef;
        private readonly ITypeIdResolver _typeIdResolver;

        internal AssemblyDefinitionReader(AssemblyDefinition assemblyDef, ITypeIdResolver typeIdResolver)
        {
            Contract.Requires(assemblyDef != null);
            Contract.Requires(typeIdResolver != null);

            _assemblyDef = assemblyDef;
            _typeIdResolver = typeIdResolver;
        }

        AssemblyId IAssemblyDefinitionReader.GetId()
        {
            Contract.Ensures(Contract.Result<AssemblyId>() != null);

            return AssemblyId.GetId(_assemblyDef.FullName);
        }

        string IAssemblyDefinitionReader.GetName()
        {
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return _assemblyDef.Name.Name;
        }

        Version IAssemblyDefinitionReader.GetVersion()
        {
            Contract.Ensures(Contract.Result<Version>() != null);

            return _assemblyDef.Name.Version;
        }

        string IAssemblyDefinitionReader.GetCulture()
        {
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return string.IsNullOrWhiteSpace(_assemblyDef.Name.Culture)
                ? "neutral"
                : _assemblyDef.Name.Culture;
        }

        byte[] IAssemblyDefinitionReader.GetPublicKeyToken()
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            return _assemblyDef.Name.HasPublicKey
                ? _assemblyDef.Name.PublicKeyToken
                : new byte[0];
        }

        IEnumerable<AssemblyId> IAssemblyDefinitionReader.GetManifestAssemblies()
        {
            Contract.Ensures(Contract.Result<IEnumerable<AssemblyId>>() != null);

            return _assemblyDef
                .Modules
                .SelectMany(m => m.AssemblyReferences)
                .Select(name => AssemblyId.GetId(name.FullName));
        }

        IEnumerable<TypeId> IAssemblyDefinitionReader.GetTypeDefinitions()
        {
            Contract.Ensures(Contract.Result<IEnumerable<TypeId>>() != null);

            return _assemblyDef
                .Modules
                .GetTypeDefinitions()
                .Select(typeDef => _typeIdResolver.GetTypeId(typeDef));
        }

        IEnumerable<TypeId> IAssemblyDefinitionReader.GetTypeReferences()
        {
            Contract.Ensures(Contract.Result<IEnumerable<TypeId>>() != null);

            return _assemblyDef
                .Modules
                .GetTypeReferences()
                .Select(typeDef => _typeIdResolver.GetTypeId(typeDef));
        }
    }
}
