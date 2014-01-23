using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.ReflectionServices;
using Lardite.RefAssistant.ReflectionServices.Data;
using System.Diagnostics.Contracts;

namespace Lardite.RefAssistant.Model.Processing.Data
{
    internal sealed class CustomAttribute : ICustomAttribute
    {
        private readonly IAssemblyService _assemblyService;
        private readonly ITypeService _typeService;

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

            _assemblyService = assemblyService;
            _typeService = typeService;
        }

        public ITypeDefinition AttributeType
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<ICustomAttributeArgument> ConstructorArguments
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<ICustomAttributeArgument> Fields
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<ICustomAttributeArgument> Properties
        {
            get { throw new NotImplementedException(); }
        }
    }
}
