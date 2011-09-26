//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

using Lardite.RefAssistant.Extensions;
using Lardite.RefAssistant.ObjectModel.Checkers.Helpers;

using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Checker of types declared in BAML
    /// </summary>
    sealed class BamlTypesChecker : ITypesChecker
    {
        #region Fields

        private ClassCheckHelper _classChecher = new ClassCheckHelper();
        private InterfaceCheckHelper _interfaceChecker = new InterfaceCheckHelper();

        #endregion // Fields

        #region Public methods

        /// <summary>
        /// Performs assembly's types checking.
        /// </summary>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        /// <param name="evaluator">The project evaluator.</param>
        public void Check(CheckerSharedData sharedData, IProjectEvaluator evaluator)
        {
            try
            {
                var declaredTypesGroups = from type in GetDeclaredTypes(evaluator)
                                          group type by type.Assembly;

                foreach (var declaredType in declaredTypesGroups)
                {
                    CheckTypesOfAssembly(sharedData, evaluator, declaredType);
                    if (!sharedData.HasCandidateReferences)
                        return;
                }
            }
            catch (Exception ex)
            {
                throw Error.CheckType(evaluator.ProjectAssembly.FullName,
                    Resources.BamlTypesChecker_CheckTypeException, ex);
            }
        }

        /// <summary>
        /// Get or set order number of checker (if several checkers exist).
        /// </summary>
        public int OrderNumber { get; set; }

        #endregion // Public methods

        #region Private methods

        /// <summary>
        /// Get types which declared in BAML in the project assembly.
        /// </summary>
        /// <param name="evaluator">The project evaluator.</param>
        /// <returns>Returns list of declared types in BAML.</returns>
        private IEnumerable<BamlTranslator.TypeDeclaration> GetDeclaredTypes(IProjectEvaluator evaluator)
        {
            IEnumerable<BamlTranslator.TypeDeclaration> declaredTypes = new List<BamlTranslator.TypeDeclaration>();

            var resources = evaluator.ProjectAssembly.Modules
               .SelectMany(m => m.Resources)
               .Where(r => r is EmbeddedResource)
               .Select(r => new ResourceReader(((EmbeddedResource)r).GetResourceStream()));

            foreach (ResourceReader reader in resources)
            {
                IDictionaryEnumerator dict = reader.GetEnumerator();
                while (dict.MoveNext())
                {
                    if (dict.Key.ToString().EndsWith(".baml"))
                    {
                        var bamlTrans = BamlTranslator.Read(dict.Value as Stream);
                        declaredTypes = declaredTypes.Union(bamlTrans.GetDeclaredTypes());
                    }
                }
            }

            return declaredTypes.Distinct(new BamlTranslator.TypeDeclarationComparer());
        }

        /// <summary>
        /// Checking assembly's types which are used in BAML.
        /// </summary>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        /// <param name="evaluator">The project evaluator.</param>
        /// <param name="declaredTypes">Declared in BAML types.</param>
        private void CheckTypesOfAssembly(CheckerSharedData sharedData, IProjectEvaluator evaluator, IEnumerable<BamlTranslator.TypeDeclaration> declaredTypes)
        {
            if (declaredTypes.Count() < 1)
            {
                return;
            }

            var assemblyDef = ReadAssembly(evaluator, declaredTypes.First().Assembly);
            if (assemblyDef == null)
            {
                return;
            }

            var assemblyTypesDefs = from typeDef in assemblyDef.Modules.GetTypesDefinitions()
                                    join declaredType in declaredTypes
                                    on typeDef.AssemblyQualifiedName().ToLower() equals declaredType.AssemblyQualifiedName.ToLower()
                                    select typeDef;

            foreach (var type in assemblyTypesDefs)
            {
                if (!sharedData.IsUsedTypeExists(type.AssemblyQualifiedName()))
                {
                    _classChecher.Check(type, sharedData);
                    _interfaceChecker.Check(type, sharedData);
                    sharedData.RemoveFromCandidates(type.Scope);

                    if (!sharedData.HasCandidateReferences)
                        return;
                }
            }
        }

        /// <summary>
        /// Assembly resolve.
        /// </summary>
        /// <param name="evaluator">The project evaluator.</param>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>Returns assembly definition or null.</returns>
        private AssemblyDefinition ReadAssembly(IProjectEvaluator evaluator, string assemblyName)
        {
            var assemblyRef = evaluator.ProjectInfo.References
                .FirstOrDefault(t => t.FullName.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase));

            if (assemblyRef == null)
            {
                assemblyName = assemblyName.Split(',')[0];
                if (assemblyName.Equals(evaluator.ProjectAssembly.FullName.Split(',')[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    return evaluator.ProjectAssembly;
                }
                return null;
            }

            var parameters = new ReaderParameters(ReadingMode.Deferred);
            parameters.AssemblyResolver = (IAssemblyResolver)evaluator.ProjectAssembly.MainModule.GetType()
                .GetField("AssemblyResolver", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField)
                .GetValue(evaluator.ProjectAssembly.MainModule);

            return AssemblyDefinition.ReadAssembly(assemblyRef.Location, parameters);
        }

        #endregion // Private methods
    }
}