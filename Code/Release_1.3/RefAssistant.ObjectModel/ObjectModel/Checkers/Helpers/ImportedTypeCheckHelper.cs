//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com),
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel.Checkers.Helpers
{
    /// <summary>
    /// Helper for checking of imported types.
    /// </summary>
    sealed class ImportedTypeCheckHelper
    {
        #region Fields

        private Dictionary<ProjectReference, AssemblyDefinition> _assemblyDefinitions = new Dictionary<ProjectReference, AssemblyDefinition>();

        #endregion // Fields

        #region Public methods

        /// <summary>
        /// Performs imported types checking.
        /// </summary>
        /// <param name="type">Specified type for checking.</param>
        /// <param name="checkerData">Used types and assemblies candidates.</param>
        public void Check(TypeDefinition type, CheckerSharedData checkerData)
        {
            foreach (ProjectReference projectReference in checkerData.CandidateReferences)
            {
                AssemblyDefinition assemblyDefinition = null;
                if (!_assemblyDefinitions.TryGetValue(projectReference, out assemblyDefinition))
                {
                    assemblyDefinition = ReadAssembly(projectReference.Location, Path.GetDirectoryName(projectReference.Location));

                    if (assemblyDefinition != null)
                    {
                        _assemblyDefinitions.Add(projectReference, assemblyDefinition);
                    }
                }

                if (assemblyDefinition != null)
                {
                    var hasType = assemblyDefinition.Modules
                        .SelectMany(m => m.Types)
                        .Count(t => t.FullName.Equals(type.FullName, StringComparison.OrdinalIgnoreCase)) > 0;

                    if (hasType)
                    {
                        checkerData.RemoveFromCandidates(projectReference);
                        break;
                    }
                }
            }
        }

        #endregion // Public methods

        #region Private methods

        private AssemblyDefinition ReadAssembly(string assemblyPath, string additionSearchDir)
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(additionSearchDir);

            return AssemblyDefinition.ReadAssembly(assemblyPath,
                new ReaderParameters(ReadingMode.Deferred) { AssemblyResolver = assemblyResolver });
        }

        #endregion // Private methods
    }
}
