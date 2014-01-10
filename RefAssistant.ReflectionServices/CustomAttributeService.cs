using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Lardite.RefAssistant.ReflectionServices.Data;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Containers;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Lookups;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Readers;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices
{
    internal sealed class CustomAttributeService : ICustomAttributeService
    {
        private readonly IAssemblyContainer _container;
        private readonly ITypeLookup _typeLookup;

        internal CustomAttributeService(IAssemblyContainer container)
        {
            ThrowUtils.ArgumentNull(() => container);

            _container = container;
            _typeLookup = new TypeLookup(container);
        }

        IEnumerable<CustomAttributeInfo> ICustomAttributeService.GetAssemblyAttributes(AssemblyId assemblyId)
        {
            ThrowUtils.ArgumentNull(() => assemblyId);

            return this.GetCustomAttributes(_container.Get(assemblyId));
        }

        IEnumerable<CustomAttributeInfo> ICustomAttributeService.GetTypeAttributes(TypeId typeId)
        {
            ThrowUtils.ArgumentNull(() => typeId);

            return this.GetCustomAttributes(_typeLookup.Get(typeId));
        }

        #region Helpers

        private IEnumerable<CustomAttributeInfo> GetCustomAttributes(ICustomAttributeProvider entity)
        {
            Contract.Requires(entity != null);

            foreach (var attribute in entity.CustomAttributes)
            {
                yield return new CustomAttributeInfo(this.CreateReader(attribute));
            }
        }

        private ICustomAttributeReader CreateReader(CustomAttribute attribute)
        {
            return new CustomAttributeReader(attribute);
        }

        #endregion
    }
}
