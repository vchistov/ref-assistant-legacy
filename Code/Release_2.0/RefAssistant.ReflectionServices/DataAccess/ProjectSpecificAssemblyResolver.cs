using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.Model.Contracts;
using Mono.Cecil;
using System.IO;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess
{
    internal sealed class ProjectSpecificAssemblyResolver : BaseAssemblyResolver
    {
        private readonly ProjectReferenceCache _references;

        public ProjectSpecificAssemblyResolver(IVsProject project)
        {
            Contract.Requires(project != null);

            var outputAssemblyDir = Path.GetDirectoryName(project.OutputAssemblyPath);
            AddSearchDirectory(outputAssemblyDir);

            _references = new ProjectReferenceCache(
                project.References, 
                new RelayEqualityComparer<AssemblyNameReference>(AreEqual));
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            Contract.Requires(name != null);
            Contract.Ensures(Contract.Result<AssemblyDefinition>() != null);

            var projectRef = _references.GetOrDefault(name);
            var assemblyDefinition = projectRef != null
                ? ReadAssembly(projectRef)
                : base.Resolve(name);

            return assemblyDefinition;
        }

        private bool AreEqual(AssemblyNameReference assemblyName1, AssemblyNameReference assemblyName2)
        {
            Contract.Requires(assemblyName1 != null);
            Contract.Requires(assemblyName2 != null);

            return string.Equals(assemblyName1.Name, assemblyName2.Name, StringComparison.OrdinalIgnoreCase);
        }

        private AssemblyDefinition ReadAssembly(VsProjectReference projectRef)
        {
            var parameters = new ReaderParameters(ReadingMode.Deferred) { AssemblyResolver = this };
            return AssemblyDefinition.ReadAssembly(projectRef.Location, parameters);
        }

        private class ProjectReferenceCache
        {
            private readonly Lazy<IList<Tuple<AssemblyNameReference, VsProjectReference>>> _references;
            private readonly IEqualityComparer<AssemblyNameReference> _comparer;

            public ProjectReferenceCache(
                IEnumerable<VsProjectReference> references,
                IEqualityComparer<AssemblyNameReference> comparer)
            {
                Contract.Requires(references != null);
                Contract.Requires(comparer != null);

                _references = new Lazy<IList<Tuple<AssemblyNameReference, VsProjectReference>>>(() => Load(references));
                _comparer = comparer;
            }

            public VsProjectReference GetOrDefault(AssemblyNameReference assemblyName)
            {
                var result = _references
                    .Value
                    .SingleOrDefault(r => _comparer.Equals(r.Item1, assemblyName));

                return result != null ? result.Item2 : null;
            }

            private IList<Tuple<AssemblyNameReference, VsProjectReference>> Load(IEnumerable<VsProjectReference> references)
            {
                Func<VsProjectReference, string> getFullName =
                    (@ref) => new ProjectAssemblyIdProvider(@ref.Location).GetId().FullName;

                return references
                    .Select(r => new Tuple<AssemblyNameReference, VsProjectReference>(
                        AssemblyNameReference.Parse(getFullName(r)),
                        r))
                    .ToList();
            }
        }
    }
}
