using System.Collections.Generic;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface ITypeReference
    {
        ITypeDefinition TypeDefinition { get; }

        IEnumerable<IField> FieldReferences { get; }

        IEnumerable<IEvent> EventReferences { get; }

        IEnumerable<IProperty> PropertyReferences { get; }        

        IEnumerable<IMethod> MethodReferences { get; }
    }
}
