using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Readers
{
    internal interface IAssemblyDefinitionReader
    {
        string GetName();

        Version GetVersion();

        string GetCulture();

        byte[] GetPublicKeyToken();

        string GetFullName();

        IEnumerable<string> GetManifestAssemblies();
    }
}
