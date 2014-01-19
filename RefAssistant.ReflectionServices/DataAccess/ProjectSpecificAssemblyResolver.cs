using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess
{
    internal sealed class ProjectSpecificAssemblyResolver : BaseAssemblyResolver
    {
        private readonly ProjectReferenceCache _references;

        public ProjectSpecificAssemblyResolver(IEnumerable<string> projectReferences, string projectOutputDir)
        {
            Contract.Requires(projectReferences != null);
            Contract.Ensures(_references != null);

            if (!string.IsNullOrWhiteSpace(projectOutputDir))
            {
                AddSearchDirectory(projectOutputDir);
            }

            _references = new ProjectReferenceCache(
                projectReferences, 
                new RelayEqualityComparer<AssemblyNameReference>(AreEqual));
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            Contract.Requires(name != null);
            Contract.Ensures(Contract.Result<AssemblyDefinition>() != null);

            var readerParams = TweakReaderParameters(parameters);

            var fileName = _references.GetOrDefault(name);
            var assemblyDefinition = fileName != null
                ? ReadAssembly(fileName, readerParams)
                : base.Resolve(name, readerParams);

            return assemblyDefinition;
        }

        private bool AreEqual(AssemblyNameReference assemblyName1, AssemblyNameReference assemblyName2)
        {
            Contract.Requires(assemblyName1 != null);
            Contract.Requires(assemblyName2 != null);

            return string.Equals(assemblyName1.Name, assemblyName2.Name, StringComparison.OrdinalIgnoreCase);
        }

        private AssemblyDefinition ReadAssembly(string fileName, ReaderParameters parameters)
        {
            Contract.Requires(fileName != null);
            Contract.Requires(parameters != null);

            return AssemblyDefinition.ReadAssembly(fileName, parameters);
        }

        private ReaderParameters TweakReaderParameters(ReaderParameters parameters)
        {
            var result = parameters ?? new ReaderParameters();

            result.ReadingMode = ReadingMode.Deferred;
            result.AssemblyResolver = result.AssemblyResolver ?? this;

            return result;
        }

        private class ProjectReferenceCache
        {
            private readonly Lazy<IList<Tuple<AssemblyNameReference, string>>> _references;
            private readonly IEqualityComparer<AssemblyNameReference> _comparer;

            public ProjectReferenceCache(
                IEnumerable<string> references,
                IEqualityComparer<AssemblyNameReference> comparer)
            {
                Contract.Requires(references != null);
                Contract.Requires(comparer != null);

                _references = new Lazy<IList<Tuple<AssemblyNameReference, string>>>(() => Load(references));
                _comparer = comparer;
            }

            public string GetOrDefault(AssemblyNameReference assemblyName)
            {
                var result = _references
                    .Value
                    .SingleOrDefault(r => _comparer.Equals(r.Item1, assemblyName));

                return result != null ? result.Item2 : null;
            }

            private IList<Tuple<AssemblyNameReference, string>> Load(IEnumerable<string> references)
            {
                Func<string, string> getFullName =
                    (fileName) => FileAssemblyIdProvider.GetId(fileName).FullName;

                return references
                    .Select(fileName => 
                        new Tuple<AssemblyNameReference, string>(AssemblyNameReference.Parse(getFullName(fileName)), fileName))
                    .ToList();
            }
        }
    }
}
