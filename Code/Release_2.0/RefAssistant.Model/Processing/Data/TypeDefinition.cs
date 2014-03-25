using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.ReflectionServices;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.Model.Processing.Data
{
    internal sealed class TypeDefinition : CustomAttributeProvider, ITypeDefinition, IEquatable<TypeDefinition>
    {
        private readonly TypeId _typeId;
        private readonly Lazy<TypeInfo> _typeInfo;
        private readonly Lazy<TypeName> _typeName;
        private readonly Lazy<IAssembly> _assembly;
        private readonly Lazy<IAssembly> _forwardedFrom;
        private readonly Lazy<IAssembly> _importedFrom;
        private readonly Lazy<ITypeDefinition> _baseType;
        private readonly Lazy<IList<ITypeDefinition>> _interfaces;

        #region .ctor
        
        internal TypeDefinition(
            IAssemblyService assemblyService,
            ITypeService typeService,
            ICustomAttributeService customAttributeService,
            TypeId typeId)
            : this(assemblyService, typeService, customAttributeService, typeId, () => typeService.GetType(typeId))
        {            
        }

        internal TypeDefinition(
            IAssemblyService assemblyService,
            ITypeService typeService,
            ICustomAttributeService customAttributeService,
            TypeInfo typeInfo)
            : this(assemblyService, typeService, customAttributeService, typeInfo.Id, () => typeInfo)
        {
        }

        private TypeDefinition(
            IAssemblyService assemblyService,
            ITypeService typeService,
            ICustomAttributeService customAttributeService,
            TypeId typeId,
            Func<TypeInfo> typeInfoFactory)
            : base(assemblyService, typeService, customAttributeService)
        {
            Contract.Requires(typeId != null);
            Contract.Requires(typeInfoFactory != null);

            _typeId = typeId;

            _typeInfo = new Lazy<TypeInfo>(typeInfoFactory);

            _typeName = new Lazy<TypeName>(() => new TypeName(this)
            {
                FullName = this.TypeInfo.FullName
            });

            _assembly = new Lazy<IAssembly>(() => CreateAssembly(this.TypeInfo.Assembly));

            _forwardedFrom = new Lazy<IAssembly>(() => CreateAssembly(this.TypeInfo.ForwardedFrom));

            _importedFrom = new Lazy<IAssembly>(() => CreateAssembly(this.TypeService.GetImportedFrom(_typeId)));

            _baseType = new Lazy<ITypeDefinition>(() => CreateTypeDefinition(this.TypeInfo.BaseType));

            _interfaces = new Lazy<IList<ITypeDefinition>>(LoadInterfaces);
        }

        #endregion

        public TypeName Name
        {
            get { return _typeName.Value; }
        }

        public ITypeDefinition BaseType
        {
            get { return _baseType.Value; }
        }

        public IEnumerable<ITypeDefinition> Interfaces
        {
            get { return _interfaces.Value; }
        }

        public IAssembly Assembly
        {
            get { return _assembly.Value; }
        }

        public IAssembly ForwardedFrom
        {
            get { return _forwardedFrom.Value; }
        }

        public IAssembly ImportedFrom
        {
            get { return _importedFrom.Value; }
        }

        public bool IsInterface
        {
            get { return this.TypeInfo.IsInterface; }
        }

        public IEnumerable<IMethod> Methods
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IField> Fields
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IProperty> Properties
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IEvent> Events
        {
            get { throw new NotImplementedException(); }
        }

        protected override IEnumerable<CustomAttributeInfo> GetCustomAttributes()
        {
            return this.CustomAttributeService.GetAttributes(_typeId);
        }

        #region Object overrides

        public bool Equals(TypeDefinition other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return _typeId.Equals(other._typeId);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TypeDefinition);
        }

        public override int GetHashCode()
        {
            return _typeId.GetHashCode();
        }

        public override string ToString()
        {
            return _typeId.ToString();
        }

        #endregion

        #region Helpers

        private TypeInfo TypeInfo
        {
            get { return _typeInfo.Value; }
        }
        
        private ITypeDefinition CreateTypeDefinition(TypeId typeId)
        {
            return typeId == null
                ? null
                : new TypeDefinition(
                    this.AssemblyService,
                    this.TypeService,
                    this.CustomAttributeService,
                    typeId);
        }

        private ITypeDefinition CreateTypeDefinition(TypeInfo typeInfo)
        {
            Contract.Requires(typeInfo != null);

            return new TypeDefinition(
                    this.AssemblyService,
                    this.TypeService,
                    this.CustomAttributeService,
                    typeInfo);
        }

        private IAssembly CreateAssembly(AssemblyId assemblyId)
        {
            return assemblyId == null
                ? null
                : new Assembly(
                    this.AssemblyService,
                    this.TypeService,
                    this.CustomAttributeService,
                    assemblyId);
        }

        private IList<ITypeDefinition> LoadInterfaces()
        {
            return this.TypeService
                .GetInterfaces(_typeId)
                .Select(info => this.CreateTypeDefinition(info))
                .ToList();
        }

        #endregion
    }
}
