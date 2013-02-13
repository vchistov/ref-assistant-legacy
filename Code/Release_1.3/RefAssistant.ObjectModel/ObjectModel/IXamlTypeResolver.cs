//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// Resolve type declared in XAML to <see cref="Mono.Cecil.TypeDefinition"/> class.
    /// </summary>
    interface IXamlTypeResolver
    {
        /// <summary>
        /// Resolve type.
        /// </summary>
        /// <param name="type">Type declaration.</param>
        /// <returns>Returns type definition of the specified type.</returns>
        TypeDefinition Resolve(XamlTypeDeclaration type);
    }
}
