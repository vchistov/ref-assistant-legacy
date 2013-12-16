using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Readers;

namespace Lardite.RefAssistant.ReflectionServices.Data.Assembly
{
    public sealed class AssemblyInfo
    {
        internal AssemblyInfo(IAssemblyDefinitionReader reader)
        {
            ThrowUtils.ArgumentNull(() => reader);
            InitInfo(reader);
        }

        public AssemblyId Id { get; private set; }

        public string Name { get; private set; }

        public Version Version { get; private set; }

        public string Culture { get; private set; }

        public PublicKeyToken PublicKeyToken { get; private set; }

        private void InitInfo(IAssemblyDefinitionReader reader)
        {
            this.Id = AssemblyId.GetId(reader.GetFullName());
            this.Name = reader.GetName();
            this.Version = reader.GetVersion();
            this.Culture = reader.GetCulture();
            this.PublicKeyToken = new PublicKeyToken(reader.GetPublicKeyToken());
        }
    }
}
