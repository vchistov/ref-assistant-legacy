using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.ReflectionServices;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.Model.Processing.Data
{
    internal sealed class Assembly : CustomAttributeProvider, IAssembly, IEquatable<Assembly>
    {
        private readonly Lazy<AssemblyInfo> _assemblyInfo;
        private readonly Lazy<IList<IAssembly>> _manifestAssemblies;
        private AssemblyId _assemblyId;

        internal Assembly(
            IAssemblyService assemblyService,
            ITypeService typeService,
            ICustomAttributeService customAttributeService,
            string fileName)
            : this(
                assemblyService,
                typeService,
                customAttributeService,
                assemblyInfoFactory: () => assemblyService.GetAssembly(fileName))
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(fileName));
        }

        internal Assembly(
            IAssemblyService assemblyService,
            ITypeService typeService,
            ICustomAttributeService customAttributeService,
            AssemblyId assemblyId)
            : this(
                assemblyService,
                typeService,
                customAttributeService,
                assemblyInfoFactory: () => assemblyService.GetAssembly(assemblyId),
                assemblyId: assemblyId)
        {
            Contract.Requires(assemblyId != null);
        }

        private Assembly(
            IAssemblyService assemblyService,
            ITypeService typeService,
            ICustomAttributeService customAttributeService,
            Func<AssemblyInfo> assemblyInfoFactory,
            AssemblyId assemblyId = null)
            : base(assemblyService, typeService, customAttributeService)
        {
            Contract.Requires(assemblyInfoFactory != null);

            _assemblyId = assemblyId;
            _assemblyInfo = new Lazy<AssemblyInfo>(assemblyInfoFactory);
            _manifestAssemblies = new Lazy<IList<IAssembly>>(LoadManifestAssemblies);
        }

        public string Name
        {
            get { return this.AssemblyInfo.Name; }
        }

        public Version Version
        {
            get { return this.AssemblyInfo.Version; }
        }

        public string Culture
        {
            get { return this.AssemblyInfo.Culture; }
        }

        public IEnumerable<byte> PublicKeyToken
        {
            get { return this.AssemblyInfo.PublicKeyToken.Token; }
        }

        public IEnumerable<IAssembly> References
        {
            get { return _manifestAssemblies.Value; }
        }

        protected override IEnumerable<CustomAttributeInfo> GetCustomAttributes()
        {
            return this.CustomAttributeService.GetAttributes(this.AssemblyId);
        }

        #region Object overrides

        public bool Equals(Assembly other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return this.AssemblyId.Equals(other.AssemblyId);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Assembly);
        }

        public override int GetHashCode()
        {
            return this.AssemblyId.GetHashCode();
        }

        public override string ToString()
        {
            return this.AssemblyId.ToString();
        }
        
        #endregion

        #region Helpers

        private AssemblyId AssemblyId
        {
            get { return _assemblyId ?? (_assemblyId = this.AssemblyInfo.Id); }
        }

        private AssemblyInfo AssemblyInfo
        {
            get { return _assemblyInfo.Value; }
        }

        private IList<IAssembly> LoadManifestAssemblies()
        {
            return this.AssemblyService
                .GetManifestAssemblies(this.AssemblyId)
                .Select(id => new Assembly(this.AssemblyService, this.TypeService, this.CustomAttributeService, id))
                .Cast<IAssembly>()
                .ToList();
        }

        #endregion
    }
}
