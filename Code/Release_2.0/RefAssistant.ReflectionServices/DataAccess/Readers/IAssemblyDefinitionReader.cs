using System;
using System.Collections.Generic;

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
