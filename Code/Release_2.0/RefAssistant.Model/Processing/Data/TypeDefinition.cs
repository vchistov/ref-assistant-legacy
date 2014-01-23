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
    internal sealed class TypeDefinition : CustomAttributeProvider, ITypeDefinition, IEquatable<TypeDefinition>
    {
        private readonly TypeId _typeId;

        internal TypeDefinition(
            IAssemblyService assemblyService,
            ITypeService typeService,
            ICustomAttributeService customAttributeService,
            TypeId typeId)
            : base(assemblyService, typeService, customAttributeService)
        {
            Contract.Requires(typeId != null);

            _typeId = typeId;
        }

        public TypeName Name
        {
            get { throw new NotImplementedException(); }
        }

        public ITypeDefinition BaseType
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<ITypeDefinition> Interfaces
        {
            get { throw new NotImplementedException(); }
        }

        public IAssembly Assembly
        {
            get { throw new NotImplementedException(); }
        }

        public IAssembly ForwardedFrom
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsInterface
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IMethod> Methods
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IMember> Fields
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IMember> Properties
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IMember> Events
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
    }
}
