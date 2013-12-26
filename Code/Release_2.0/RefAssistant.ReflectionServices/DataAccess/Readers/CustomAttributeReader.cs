using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.ReflectionServices.Data;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Readers
{
    internal class CustomAttributeReader : ICustomAttributeReader
    {
        private readonly CustomAttribute _customAttribute;

        internal CustomAttributeReader(CustomAttribute customAttribute)
        {
            Contract.Requires(customAttribute != null);
            _customAttribute = customAttribute;
        }

        public TypeId GetAttributeType()
        {
            return GetTypeId(_customAttribute.AttributeType);
        }

        public IEnumerable<TypeId> GetConstructorArguments()
        {
            return EnumerateArgments(_customAttribute.ConstructorArguments);
        }

        public IEnumerable<TypeId> GetFields()
        {
            return EnumerateArgments(_customAttribute.Fields.Select(f => f.Argument));
        }

        public IEnumerable<TypeId> GetProperties()
        {
            return EnumerateArgments(_customAttribute.Properties.Select(p => p.Argument));
        }

        #region Private methods
        
        private TypeId GetTypeId(TypeReference typeRef)
        {
            var assemblyId = AssemblyId.GetId(typeRef.Scope.GetAssemblyNameReference().FullName);
            var typeId = TypeId.GetId(assemblyId, typeRef.FullName);

            return typeId;
        }

        private IEnumerable<TypeId> EnumerateArgments(IEnumerable<CustomAttributeArgument> args)
        {
            foreach (var arg in args)
            {
                var typeRef = arg.Value is TypeReference
                    ? (TypeReference)arg.Value
                    : arg.Type;

                yield return GetTypeId(typeRef);
            }
        }

        #endregion
    }
}
