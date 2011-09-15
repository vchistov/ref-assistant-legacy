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
    /// Helper for type's interfaces checking.
    /// </summary>
    sealed class InterfaceCheckHelper
    {
        #region Fields

        private ImportedTypeCheckHelper _importedCheckHelper = new ImportedTypeCheckHelper();

        #endregion // Fields

        #region Public methods

        /// <summary>
        /// Performs checking of type's interfaces.
        /// </summary>
        /// <param name="type">Specified type for checking.</param>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        public void Check(TypeDefinition type, CheckerSharedData sharedData)
        {
            try
            {
                IMetadataScope forwardedFrom;
                foreach (TypeReference interfaceRef in type.GetInterfaces())
                {
                    var interfaceDef = interfaceRef.Resolve(out forwardedFrom);
                    sharedData.RemoveFromCandidates(forwardedFrom);

                    if (interfaceDef != null)
                    {
                        if (interfaceDef.IsImport)
                        {
                            _importedCheckHelper.Check(interfaceDef, sharedData);
                        }
                        else
                        {
                            sharedData.RemoveFromCandidates(interfaceDef.Scope);
                        }
                        sharedData.AddToUsedTypes(interfaceDef.AssemblyQualifiedName());
                    }

                    if (!sharedData.HasCandidateReferences)
                        return;
                }
            }
            catch (Exception ex)
            {
                throw Error.CheckType(type.AssemblyQualifiedName(),
                    string.Format(Resources.CleanExecutor_CheckTypeInterfacesException, type.AssemblyQualifiedName()), ex);
            }
        }

        #endregion // Public methods
    }
}
