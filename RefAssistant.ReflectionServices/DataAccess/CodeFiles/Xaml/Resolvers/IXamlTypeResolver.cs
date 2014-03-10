using System.Xaml;
using Lardite.RefAssistant.ReflectionServices.Data;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles.Xaml.Resolvers
{
    internal interface IXamlTypeResolver
    {
        TypeId Resolve(XamlType xamlType);
    }
}
