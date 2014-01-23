using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.ReflectionServices;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.Model.Processing.Data
{
    internal sealed class CustomAttribute : ICustomAttribute
    {
        private Lazy<ITypeDefinition> _attributeType;
        private Lazy<IList<ITypeDefinition>> _ctorArguments;
        private Lazy<IList<ITypeDefinition>> _fields;
        private Lazy<IList<ITypeDefinition>> _properties;

        internal CustomAttribute(
            IAssemblyService assemblyService, 
            ITypeService typeService,
            ICustomAttributeService customAttributeService,
            CustomAttributeInfo customAttributeInfo)
        {
            Contract.Requires(assemblyService != null);
            Contract.Requires(typeService != null);
            Contract.Requires(customAttributeService != null);
            Contract.Requires(customAttributeInfo != null);

            _attributeType = new Lazy<ITypeDefinition>(
                () => new TypeDefinition(
                    assemblyService, 
                    typeService, 
                    customAttributeService, 
                    customAttributeInfo.AttributeType));

            _ctorArguments = new Lazy<IList<ITypeDefinition>>(
                () => LoadArgumentTypes(
                    assemblyService,
                    typeService,
                    customAttributeService,
                    customAttributeInfo.ConstructorArguments));

            _fields = new Lazy<IList<ITypeDefinition>>(
                () => LoadArgumentTypes(
                    assemblyService,
                    typeService,
                    customAttributeService,
                    customAttributeInfo.Fields));

            _properties = new Lazy<IList<ITypeDefinition>>(
                () => LoadArgumentTypes(
                    assemblyService,
                    typeService,
                    customAttributeService,
                    customAttributeInfo.Properties));
        }

        public ITypeDefinition AttributeType
        {
            get { return _attributeType.Value; }
        }

        public IEnumerable<ITypeDefinition> ConstructorArguments
        {
            get { return _ctorArguments.Value; }
        }

        public IEnumerable<ITypeDefinition> Fields
        {
            get { return _fields.Value; }
        }

        public IEnumerable<ITypeDefinition> Properties
        {
            get { return _properties.Value; }
        }

        #region Helpers

        private IList<ITypeDefinition> LoadArgumentTypes(
            IAssemblyService assemblyService,
            ITypeService typeService,
            ICustomAttributeService customAttributeService,
            IEnumerable<TypeId> typeIds)
        {
            return typeIds
                .Select(id => new TypeDefinition(assemblyService, typeService, customAttributeService, id))
                .Cast<ITypeDefinition>()
                .ToList();
        }

        #endregion
    }
}
