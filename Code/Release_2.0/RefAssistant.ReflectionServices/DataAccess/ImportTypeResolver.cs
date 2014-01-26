using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Lardite.RefAssistant.ReflectionServices.Data;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Containers;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Lookups;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess
{
    internal class ImportTypeResolver : IImportTypeResolver
    {
        private const string TypeIdentifierAttributeName = "System.Runtime.InteropServices.TypeIdentifierAttribute";

        private readonly InteropAssembliesProvider _interopAssemblyProvider;
        private readonly ITypeLookup _typeLookup;
        private readonly GenericCache<TypeId, TypeDefinition> _importTypeCache = new GenericCache<TypeId,TypeDefinition>();

        internal ImportTypeResolver(IAssemblyContainer container, IEnumerable<string> projectReferences)
        {
            Contract.Requires(container != null);
            Contract.Requires(projectReferences != null);

            _interopAssemblyProvider = new InteropAssembliesProvider(container, projectReferences);
            _typeLookup = new TypeLookup(container);
        }

        public bool IsImport(TypeId typeId)
        {
            Contract.Requires(typeId != null);

            if (_importTypeCache.Contains(typeId))
                return true;

            var typeDef = _typeLookup.Get(typeId);
            Contract.Assert(typeDef != null);

            return typeDef.IsImport
                || IsValueTypeImport(typeDef);
        }

        public TypeDefinition Resolve(TypeId typeId)
        {
            Contract.Requires(typeId != null);

            return _importTypeCache.GetOrAdd(typeId, LookupTypeImport);
        }

        #region Helpers

        private bool IsValueTypeImport(TypeDefinition typeDef)
        {
            return typeDef.IsValueType
                && typeDef.HasCustomAttribute(TypeIdentifierAttributeName)
                && typeDef.IsCompilerGenerated();
        }

        private TypeDefinition LookupTypeImport(TypeId typeId)
        {
            var typeDef = _typeLookup.Get(typeId);
            Contract.Assert(typeDef != null);

            TypeDefinition target;
            if (TryResolveByTypeIdentifier(typeDef, out target))
            {
                return target;
            }

            Guid? filterGuid = typeDef.GetGuid();
            foreach (var assemblyDef in _interopAssemblyProvider.InteropAssemblies)
            {
                target = SearchTypeDefinition(assemblyDef, typeDef.FullName, filterGuid, typeDef.IsImport);
                if (target != null)
                {
                    return target;
                }
            }

            throw new TypeImportNotFoundException(typeId);
        }

        private bool TryResolveByTypeIdentifier(TypeDefinition typeDef, out TypeDefinition target)
        {
            target = null;

            var attribute = typeDef
                .GetCustomAttributes(TypeIdentifierAttributeName)
                .SingleOrDefault();

            if (attribute != null && attribute.ConstructorArguments.Count == 2)
            {
                Guid scope;
                if (Guid.TryParse((string)attribute.ConstructorArguments[0].Value, out scope))
                {
                    string identifier = (string)attribute.ConstructorArguments[1].Value;
                    var assemblyDef = _interopAssemblyProvider.GetAssembly(scope);

                    if (assemblyDef != null)
                    {
                        target = SearchTypeDefinition(assemblyDef, identifier, typeDef.GetGuid(), typeDef.IsImport);
                        return target != null;
                    }
                }
            }

            return false;
        }

        private TypeDefinition SearchTypeDefinition(AssemblyDefinition assemblyDef, string typeFullName, Guid? typeGuid, bool isImport)
        {
            return assemblyDef.Modules.GetTypeDefinitions()
                    .FirstOrDefault(t => AreNamesEqualFilter(t, typeFullName)
                        && AreGuidsEqualFilter(t, typeGuid)
                        && t.IsImport == isImport);
        }

        private bool AreNamesEqualFilter(TypeDefinition current, string filterFullName)
        {
            return string.Equals(current.FullName, filterFullName, StringComparison.OrdinalIgnoreCase);
        }

        private bool AreGuidsEqualFilter(TypeDefinition current, Guid? filterGuid)
        {
            return filterGuid.HasValue
                ? filterGuid.Value.Equals(current.GetGuid())
                : true;
        }

        #endregion

        #region Nested types

        class InteropAssembliesProvider
        {
            private readonly Lazy<IList<AssemblyDefinition>> _interopAssemblies;

            public InteropAssembliesProvider(IAssemblyContainer container, IEnumerable<string> projectReferences)
            {
                _interopAssemblies = new Lazy<IList<AssemblyDefinition>>(
                    () => LoadInteropAssemblies(container, projectReferences).ToList());
            }

            public AssemblyDefinition GetAssembly(Guid guid)
            {
                return this.InteropAssemblies
                    .FirstOrDefault(asm => guid.Equals(asm.GetGuid()));
            }

            public IEnumerable<AssemblyDefinition> InteropAssemblies
            {
                get { return _interopAssemblies.Value; }
            }

            #region Helpers

            private IEnumerable<AssemblyDefinition> LoadInteropAssemblies(IAssemblyContainer container, IEnumerable<string> projectReferences)
            {
                foreach (var fileName in projectReferences)
                {
                    var id = FileAssemblyIdProvider.GetId(fileName);
                    var assemblyDef = container.Get(id);

                    Contract.Assert(assemblyDef != null);

                    if (IsInteropAssembly(assemblyDef))
                    {
                        yield return assemblyDef;
                    }
                }
            }

            private bool IsInteropAssembly(AssemblyDefinition assemblyDef)
            {
                return assemblyDef.HasCustomAttribute("System.Runtime.InteropServices.ImportedFromTypeLibAttribute")
                    || assemblyDef.HasCustomAttribute("System.Runtime.InteropServices.PrimaryInteropAssemblyAttribute");
            }

            #endregion
        }

        #endregion
    }
}

