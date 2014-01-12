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
        private readonly ITypeIdProvider _typeIdProvider;

        internal CustomAttributeReader(CustomAttribute customAttribute, ITypeIdProvider typeIdProvider)
        {
            Contract.Requires(customAttribute != null);
            Contract.Requires(typeIdProvider != null);

            _customAttribute = customAttribute;
            _typeIdProvider = typeIdProvider;
        }

        TypeId ICustomAttributeReader.GetAttributeType()
        {
            return _typeIdProvider.GetId(_customAttribute.AttributeType);
        }

        IEnumerable<TypeId> ICustomAttributeReader.GetConstructorArguments()
        {
            return EnumerateArgments(_customAttribute.ConstructorArguments);
        }

        IEnumerable<TypeId> ICustomAttributeReader.GetFields()
        {
            return EnumerateArgments(_customAttribute.Fields.Select(f => f.Argument));
        }

        IEnumerable<TypeId> ICustomAttributeReader.GetProperties()
        {
            return EnumerateArgments(_customAttribute.Properties.Select(p => p.Argument));
        }

        #region Helpers
        
        private IEnumerable<TypeId> EnumerateArgments(IEnumerable<CustomAttributeArgument> args)
        {
            foreach (var arg in args)
            {
                var typeRef = arg.Value is TypeReference
                    ? (TypeReference)arg.Value
                    : arg.Type;

                yield return _typeIdProvider.GetId(typeRef);
            }
        }

        #endregion
    }
}
