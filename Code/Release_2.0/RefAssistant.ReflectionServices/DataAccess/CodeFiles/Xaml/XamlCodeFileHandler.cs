using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Xaml;
using Lardite.RefAssistant.ReflectionServices.Data;
using Microsoft.Xaml.Tools.XamlDom;
using Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles.Xaml.Resolvers;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles.Xaml
{
    internal sealed class XamlCodeFileHandler : ICodeFileHandler
    {
        private readonly Stream _fileStream;
        private readonly IXamlTypeResolver[] _typeResolvers;        
        private readonly XamlSchemaContext _context;

        public XamlCodeFileHandler(Stream fileStream, IEnumerable<IXamlTypeResolver> typeResolvers)        
        {
            Contract.Requires(fileStream != null);
            Contract.Requires(fileStream.CanRead);
            Contract.Requires(typeResolvers != null);

            _fileStream = fileStream;
            _typeResolvers = typeResolvers.ToArray();
            _context = new SimpleXamlSchemaContext();
        }

        public IEnumerable<TypeId> ResolveReferencedTypes()
        {
            Contract.Assert(_fileStream.CanRead);

            using (var xamlReader = new XamlXmlReader(_fileStream, _context, new XamlXmlReaderSettings { CloseInput = false }))
            {
                XamlDomObject rootNode = XamlDomServices.Load(xamlReader) as XamlDomObject;
                if (rootNode != null)
                {
                    foreach (var xamlType in rootNode.DescendantsAndSelf())
                    {
                        var typeId = _typeResolvers
                            .Select(r => r.Resolve(xamlType.Type))
                            .FirstOrDefault(t => t != null);

                        if (typeId != null)
                        {
                            yield return typeId;
                        }
                    }
                }
            }
        }
    }
}
