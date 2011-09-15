//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using System;
using Lardite.RefAssistant.Extensions;
using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel.Checkers.Helpers
{
    /// <summary>
    /// Helper for checking class hierarchy.
    /// </summary>
    sealed class ClassCheckHelper
    {
        #region Public methods

        /// <summary>
        /// Performs class hierarchy checking.
        /// </summary>
        /// <param name="type">Specified type for checking.</param>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        public void Check(TypeDefinition type, CheckerSharedData sharedData)
        {
            try
            {
                if (type == null)
                    return;

                var baseType = type.BaseType;
                if (baseType == null || sharedData.UsedTypes.Contains(baseType.AssemblyQualifiedName()))
                {
                    sharedData.AddToUsedTypes(type.AssemblyQualifiedName());
                    return;
                }

                IMetadataScope forwardedFrom;
                var baseTypeDef = baseType.Resolve(out forwardedFrom);
                sharedData.RemoveFromCandidates(forwardedFrom);
                if (baseTypeDef != null)
                {
                    sharedData.RemoveFromCandidates(baseTypeDef.Scope);
                }
                
                Check(baseTypeDef, sharedData);
                sharedData.AddToUsedTypes(type.AssemblyQualifiedName());
            }
            catch (Exception ex)
            {
                throw Error.CheckType(type.AssemblyQualifiedName(),
                    string.Format(Resources.CleanExecutor_CheckTypeHierarchyException, type.AssemblyQualifiedName()), ex);
            }
        }

        #endregion // Public methods
    }
}
