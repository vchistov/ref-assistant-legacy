using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Readers;

namespace Lardite.RefAssistant.ReflectionServices.Data
{
    [Serializable]
    [DebuggerDisplay("{FullName}")]
    public sealed class TypeInfo
    {
        internal TypeInfo(ITypeDefinitionReader reader)
        {
            Contract.Requires(reader != null);

            this.Id = reader.GetId();
            this.BaseType = reader.GetBaseType();
            this.IsInterface = reader.IsInterface;

            this.Assembly = this.Id.AssemblyId;
            this.ForwardedFrom = this.Id.ForwardedFromId;
            this.FullName = this.Id.FullName;
        }

        public TypeId Id { get; private set; }

        public TypeId BaseType { get; private set; }

        public AssemblyId Assembly { get; private set; }

        public string FullName { get; private set; }

        public bool IsInterface { get; private set; }

        public AssemblyId ForwardedFrom { get; private set; }
    }
}
