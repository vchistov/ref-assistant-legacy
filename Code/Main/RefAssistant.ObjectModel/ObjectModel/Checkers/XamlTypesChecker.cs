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
using System.Resources;

using Lardite.RefAssistant.Extensions;
using Lardite.RefAssistant.ObjectModel.Checkers.Helpers;

using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Checker of types declared in BAML
    /// </summary>
    sealed class XamlTypesChecker : ITypesChecker
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
                var reader = new ResourcesReader(evaluator);
                CheckXamlDeclaredTypes(sharedData, evaluator, reader.GetXamlDeclaredTypes());
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
        /// Checking assembly's types which are used in BAML.
        /// </summary>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        /// <param name="evaluator">The project evaluator.</param>
        /// <param name="declaredTypes">Declared in BAML types.</param>
        private void CheckXamlDeclaredTypes(CheckerSharedData sharedData, IProjectEvaluator evaluator, IEnumerable<XamlTypeDeclaration> declaredTypes)
        {
            var xamlTypeResolver = new XamlTypeResolver(evaluator);
            foreach (var type in declaredTypes)
            {
                var typeDef = xamlTypeResolver.Resolve(type);
                if (typeDef != null && !sharedData.IsUsedTypeExists(typeDef.AssemblyQualifiedName()))
                {
                    _classChecher.Check(typeDef, sharedData);
                    _interfaceChecker.Check(typeDef, sharedData);
                    sharedData.RemoveFromCandidates(typeDef.Scope);

                    if (!sharedData.HasCandidateReferences)
                        return;
                }
            }
        }

        #endregion // Private methods

        #region Nested types

        private class ResourcesReader
        {
            #region Fields

            private readonly IProjectEvaluator _evaluator;

            #endregion // Fields

            #region .ctor

            public ResourcesReader(IProjectEvaluator evaluator)
            {
                _evaluator = evaluator;
            }

            #endregion // .ctor

            #region Public methods

            /// <summary>
            /// Get types which declared in XAML(BAML) in the project assembly.
            /// </summary>
            /// <param name="evaluator">The project evaluator.</param>
            /// <returns>Returns list of declared types in XAML(BAML).</returns>
            public IEnumerable<XamlTypeDeclaration> GetXamlDeclaredTypes()
            {
                IEnumerable<XamlTypeDeclaration> declaredTypes = new List<XamlTypeDeclaration>();

                var resources = _evaluator.ProjectAssembly.Modules
                   .SelectMany(m => m.Resources)
                   .Where(r => r is EmbeddedResource)
                   .Select(r => (EmbeddedResource)r);

                foreach (var resource in resources)
                {
                    declaredTypes = declaredTypes.Union(ReadResource(resource));
                }

                return declaredTypes.Distinct();
            }

            #endregion // Public methods

            #region Private methods

            private IEnumerable<XamlTypeDeclaration> ReadResource(EmbeddedResource resource)
            {
                if (resource.Name.EndsWith(".resources", StringComparison.InvariantCultureIgnoreCase))
                {
                    IEnumerable<XamlTypeDeclaration> declaredTypes = new List<XamlTypeDeclaration>();
                    var resourceReader = new ResourceReader(resource.GetResourceStream());
                    IDictionaryEnumerator dict = resourceReader.GetEnumerator();
                    while (dict.MoveNext())
                    {
                        foreach (var typeDeclaration in ReadXamlStream(dict.Key.ToString(), dict.Value as Stream))
                        {
                            yield return typeDeclaration;
                        }
                    }
                }
                else
                {
                    foreach (var typeDeclaration in ReadXamlStream(resource.Name, resource.GetResourceStream()))
                    {
                        yield return typeDeclaration;
                    }
                }
            }

            private IEnumerable<XamlTypeDeclaration> ReadXamlStream(string key, Stream stream)
            {
                IXamlAnalyser analyser = null;
                try
                {
                    analyser = CreateXamlAnalyser(key, stream);
                    return analyser.GetDeclaredTypes().ToList();
                }
                finally
                {
                    if (analyser is IDisposable)
                    {
                        ((IDisposable)analyser).Dispose();
                    }
                }
            }

            private IXamlAnalyser CreateXamlAnalyser(string key, Stream stream)
            {
                if (key.EndsWith(".baml", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new BamlAnalyser(stream);
                }
                else if (key.EndsWith(".xaml", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new NativeXamlAnalyser(stream);
                }

                return NullXamlAnalyser.Instance;
            }

            #endregion // Private methods

        }

        private class NullXamlAnalyser : IXamlAnalyser
        {
            private static IEnumerable<XamlTypeDeclaration> EmptyList = new List<XamlTypeDeclaration>();

            public readonly static NullXamlAnalyser Instance = new NullXamlAnalyser();

            public IEnumerable<XamlTypeDeclaration> GetDeclaredTypes()
            {
                return EmptyList;
            }
        }

        #endregion // Nested types
    }
}