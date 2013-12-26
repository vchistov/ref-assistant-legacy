using System;
using System.Collections.Generic;
using Lardite.RefAssistant.ReflectionServices.Data;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Containers;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Readers;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices
{
    internal sealed class CustomAttributeService : ICustomAttributeService
    {
        private readonly IAssemblyContainer _container;

        internal CustomAttributeService(IAssemblyContainer container)
        {
            ThrowUtils.ArgumentNull(() => container);

            _container = container;
        }

        IEnumerable<CustomAttributeInfo> ICustomAttributeService.GetAssemblyAttributes(AssemblyId assemblyId)
        {
            ThrowUtils.ArgumentNull(() => assemblyId);

            var assembly = _container.Get(assemblyId);
            foreach (var attribute in assembly.CustomAttributes)
            {
                yield return new CustomAttributeInfo(this.CreateReader(attribute));
            }
        }

        IEnumerable<CustomAttributeInfo> ICustomAttributeService.GetTypeAttributes(TypeId typeId)
        {
            // TODO: need to implement typecontainer?
            throw new NotImplementedException();
        }

        private ICustomAttributeReader CreateReader(CustomAttribute attribute)
        {
            return new CustomAttributeReader(attribute);
        }
    }
}
