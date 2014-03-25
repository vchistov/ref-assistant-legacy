using System;
using System.Collections.Generic;
using System.Linq;

namespace Lardite.RefAssistant.Algorithms.Contracts
{
    internal static class TypeDefinitionExtensions
    {
        public static IEnumerable<IMethod> GetMethodWithOverloads(this ITypeDefinition @this, IMethod method)
        {
            ThrowUtils.ArgumentNull(() => @this);
            ThrowUtils.ArgumentNull(() => method);

            if (@this.Methods.HasElements())
            {
                var paramsCount = method.Parameters.AsNotNull().Count();
                return @this.Methods.Where(m =>
                    string.Equals(m.Name, method.Name, StringComparison.OrdinalIgnoreCase)
                    && m.Parameters.AsNotNull().Count() == paramsCount);
            }

            return Enumerable.Empty<IMethod>();
        }
    }
}
