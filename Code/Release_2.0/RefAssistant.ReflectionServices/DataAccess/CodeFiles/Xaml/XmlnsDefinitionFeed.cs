using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles.Xaml
{
    internal static class XmlnsDefinitionFeed
    {
        private const string XmlnsDefinitionAttributeName = "System.Windows.Markup.XmlnsDefinitionAttribute";

        public static bool HasXmlnsDefinitions(AssemblyDefinition assemblyDef)
        {
            Contract.Requires(assemblyDef != null);
            return assemblyDef.HasCustomAttribute(XmlnsDefinitionAttributeName);
        }

        public static IEnumerable<XmlnsDefinitionInfo> GetXmlnsDefinitions(AssemblyDefinition assemblyDef)
        {
            Contract.Requires(assemblyDef != null);

            return assemblyDef
                .GetCustomAttributes(XmlnsDefinitionAttributeName)
                .Select(ca => new XmlnsDefinitionInfo(ca));
        }
    }
}
