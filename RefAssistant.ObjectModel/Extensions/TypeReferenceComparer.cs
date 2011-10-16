//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;

using Mono.Cecil;

namespace Lardite.RefAssistant.Extensions
{
    /// <summary>
    /// The <see cref="Mono.Cecil.TypeDefinition"/> comparer.
    /// </summary>
    sealed class TypeReferenceComparer : IEqualityComparer<TypeReference>
    {
        /// <summary>
        /// Equals type definitions.
        /// </summary>
        /// <param name="ref1">TD1.</param>
        /// <param name="ref2">TD2.</param>
        /// <returns>Result.</returns>
        public bool Equals(TypeReference ref1, TypeReference ref2)
        {
            if (ReferenceEquals(ref1, ref2))
            {
                return true;
            }
            if ((ref1 != null && ref2 == null) || (ref1 == null && ref2 != null))
            {
                return false;
            }

            return ref1.AssemblyQualifiedName()
                .Equals(ref2.AssemblyQualifiedName(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets hash code.
        /// </summary>
        /// <param name="a">Type definition.</param>
        /// <returns>Hash code.</returns>
        public int GetHashCode(TypeReference a)
        {            
            return (a.AssemblyQualifiedName().GetHashCode());
        }
    }
}
