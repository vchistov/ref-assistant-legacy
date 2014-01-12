using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Readers;

namespace Lardite.RefAssistant.ReflectionServices.Data
{
    [Serializable]
    [DebuggerDisplay("{Name}")]
    public sealed class AssemblyInfo
    {
        internal AssemblyInfo(IAssemblyDefinitionReader reader)
        {
            Contract.Requires(reader != null);

            this.Id = reader.GetId();
            this.Name = reader.GetName();
            this.Version = reader.GetVersion();
            this.Culture = reader.GetCulture();
            this.PublicKeyToken = new PublicKeyToken(reader.GetPublicKeyToken());
        }

        public AssemblyId Id { get; private set; }

        public string Name { get; private set; }

        public Version Version { get; private set; }

        public string Culture { get; private set; }

        public PublicKeyToken PublicKeyToken { get; private set; }
    }
}
