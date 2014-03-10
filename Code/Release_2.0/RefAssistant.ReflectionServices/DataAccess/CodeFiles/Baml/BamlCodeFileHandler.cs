using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles.Baml
{
    internal sealed class BamlCodeFileHandler : ICodeFileHandler
    {
        private readonly Stream _fileStream;

        internal BamlCodeFileHandler(Stream fileStream)
        {
            Contract.Requires(fileStream != null);
            Contract.Requires(fileStream.CanRead);

            _fileStream = fileStream;
        }

        public IEnumerable<TypeId> ResolveReferencedTypes()
        {
            Contract.Assert(_fileStream.CanRead);

            var translator = new BamlTranslator(_fileStream);
            return translator.GetDeclaredTypes().Select(t => ToTypeId(t));
        }

        #region Helpers

        private static TypeId ToTypeId(BamlTranslator.TypeDeclaration typeDeclaration)
        {
            // the type in xaml cannot be nested, so we don't need to transform the name to cecil naming format.            
            string fullName = string.IsNullOrWhiteSpace(typeDeclaration.Namespace)
                ? typeDeclaration.Name
                : string.Format("{0}.{1}", typeDeclaration.Namespace, typeDeclaration.Name);

            return TypeId.GetId(fullName, AssemblyId.GetId(typeDeclaration.Assembly));
        }

        #endregion
    }
}
