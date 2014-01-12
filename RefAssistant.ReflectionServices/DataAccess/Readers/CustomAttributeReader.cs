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
        private readonly ITypeIdResolver _typeIdResolver;

        internal CustomAttributeReader(CustomAttribute customAttribute, ITypeIdResolver typeIdResolver)
        {
            Contract.Requires(customAttribute != null);
            Contract.Requires(typeIdResolver != null);

            _customAttribute = customAttribute;
            _typeIdResolver = typeIdResolver;
        }

        TypeId ICustomAttributeReader.GetAttributeType()
        {
            return _typeIdResolver.GetTypeId(_customAttribute.AttributeType);
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

                yield return _typeIdResolver.GetTypeId(typeRef);
            }
        }

        #endregion
    }
}
