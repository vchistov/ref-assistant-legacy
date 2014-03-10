using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;

namespace Lardite.RefAssistant.ReflectionServices.DataAccess.CodeFiles.Xaml
{
    internal sealed class XmlnsDefinitionInfo
    {
        private readonly CustomAttribute _customAttribute;

        internal XmlnsDefinitionInfo(CustomAttribute customAttribute)
        {
            Contract.Requires(customAttribute != null);
            Contract.Requires(
                string.Equals(
                    customAttribute.AttributeType.FullName, 
                    "System.Windows.Markup.XmlnsDefinitionAttribute", 
                    StringComparison.OrdinalIgnoreCase));

            _customAttribute = customAttribute;
        }

        public string AssemblyName
        {
            get
            {
                var property = _customAttribute
                    .Properties
                    .FirstOrDefault(p => string.Equals(p.Name, "AssemblyName", StringComparison.Ordinal));

                return (string)property.Argument.Value;
            }
        }

        public string ClrNamespace
        {
            get { return (string)_customAttribute.ConstructorArguments[1].Value; }
        }

        public string XmlNamespace
        {
            get { return (string)_customAttribute.ConstructorArguments[0].Value; }
        }
    }
}
