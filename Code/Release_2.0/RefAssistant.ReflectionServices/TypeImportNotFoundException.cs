using System;
using System.Runtime.Serialization;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.ReflectionServices
{
    [Serializable]
    public sealed class TypeImportNotFoundException : ApplicationException
    {
        private const string NotFoundMessage = "The type import '{0}' not found.";

        public TypeImportNotFoundException(TypeId typeId)
            : this(typeId, string.Format(NotFoundMessage, typeId.FullName), null)
        {
        }
        
        public TypeImportNotFoundException(TypeId typeId, string message)
            : this(typeId, message, null)
        { 
        }

        public TypeImportNotFoundException(TypeId typeId, string message, Exception inner)
            : base(message, inner)
        {
            this.TypeId = typeId;
        }

        private TypeImportNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { 
        }

        public TypeId TypeId
        {
            get { return (TypeId)this.Data["TypeId"]; }
            private set { this.Data["TypeId"] = value; }
        }
    }
}
