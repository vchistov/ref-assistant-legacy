using System;
using System.Collections.Generic;
using System.Linq;
using System.Xaml;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles.Xaml
{
    internal sealed class SimpleXamlSchemaContext : XamlSchemaContext
    {
        public override IEnumerable<string> GetAllXamlNamespaces()
        {
            yield break;
        }

        public override ICollection<XamlType> GetAllXamlTypes(string xamlNamespace)
        {
            return Enumerable.Empty<XamlType>().ToList();
        }

        protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
        {
            return new XamlType(xamlNamespace, name, typeArguments, this);
        }

        public override string GetPreferredPrefix(string xmlns)
        {
            return null;
        }

        public override XamlType GetXamlType(Type type)
        {
            return new XamlType(type, this);
        }
    }
}
