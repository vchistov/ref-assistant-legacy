using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.ReflectionServices;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.Model.Processing.Data
{
    internal abstract class CustomAttributeProvider : ICustomAttributeProvider
    {
        private readonly Lazy<IList<ICustomAttribute>> _customAttribute;

        protected CustomAttributeProvider(
            IAssemblyService assemblyService,
            ITypeService typeService,
            ICustomAttributeService customAttributeService)
        {
            Contract.Requires(assemblyService != null);
            Contract.Requires(typeService != null);
            Contract.Requires(customAttributeService != null);

            this.AssemblyService = assemblyService;
            this.TypeService = typeService;
            this.CustomAttributeService = customAttributeService;

            _customAttribute = new Lazy<IList<ICustomAttribute>>(LoadCustomAttributes);
        }

        public IEnumerable<ICustomAttribute> CustomAttributes
        {
            get { return _customAttribute.Value; }
        }

        protected abstract IEnumerable<CustomAttributeInfo> GetCustomAttributes();

        protected IAssemblyService AssemblyService { get; private set; }

        protected ITypeService TypeService { get; private set; }

        protected ICustomAttributeService CustomAttributeService { get; private set; }

        #region Helpers

        private IList<ICustomAttribute> LoadCustomAttributes()
        {
            return this.GetCustomAttributes()
                .Select(info => new CustomAttribute(this.AssemblyService, this.TypeService, this.CustomAttributeService, info))
                .Cast<ICustomAttribute>()
                .ToList();
        }

        #endregion
    }
}
