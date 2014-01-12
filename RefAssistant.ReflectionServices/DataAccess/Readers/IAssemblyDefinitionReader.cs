using System;
using System.Collections.Generic;
using Mono.Cecil;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Readers
{
    internal interface IAssemblyDefinitionReader
    {
        AssemblyId GetId();

        string GetName();

        Version GetVersion();

        string GetCulture();

        byte[] GetPublicKeyToken();

        IEnumerable<AssemblyId> GetManifestAssemblies();

        IEnumerable<TypeId> GetTypeDefinitions();

        IEnumerable<TypeId> GetTypeReferences();
    }
}
