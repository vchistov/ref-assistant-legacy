using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.ReflectionServices.Data;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Containers;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.Lookups
{
    internal sealed class TypeLookup : ITypeLookup
    {
        private readonly IAssemblyContainer _container;

        internal TypeLookup(IAssemblyContainer container)
        {
            Contract.Requires(container != null);

            _container = container;
        }

        TypeDefinition ITypeLookup.Get(TypeId typeId)
        {
            Contract.Requires(typeId != null);
            Contract.Ensures(Contract.Result<TypeDefinition>() != null);

            var assembly = _container.Get(typeId.AssemblyId);

            Contract.Assert(assembly != null);

            return assembly.GetType(typeId.FullName);
        }        
    }
}
