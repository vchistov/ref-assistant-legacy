using System.Collections.Generic;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Contracts;
using Moq;

namespace Lardite.RefAssistant.Algorithms.UnitTests
{
    internal static class MockUtils
    {
        public static Mock<IAssembly> CreateAssemblyMock(string name)
        {
            var assembly = new Mock<IAssembly>();

            assembly.SetupGet<string>(p => p.Name).Returns(name);
            assembly.Setup(m => m.Equals(It.IsAny<IAssembly>())).Returns<IAssembly>((other) => string.Equals(other.Name, name));

            return assembly;
        }

        public static Mock<ITypeDefinition> CreateTypeMock(
            string fullName, 
            IAssembly assembly, 
            ITypeDefinition baseType = null,
            IAssembly forwardedFrom = null,
            bool isInterface = false,
            IEnumerable<ITypeDefinition> interfaces = null)
        {
            return CreateTypeMock<ITypeDefinition>(fullName, baseType, assembly, forwardedFrom, isInterface, interfaces);
        }

        public static Mock<ITypeImport> CreateTypeMock(
            string fullName,
            IAssembly assembly,
            IAssembly importedFrom,
            ITypeDefinition baseType = null,
            IAssembly forwardedFrom = null,
            bool isInterface = false,
            IEnumerable<ITypeDefinition> interfaces = null)
        {
            var type = CreateTypeMock<ITypeImport>(fullName, baseType, assembly, forwardedFrom, isInterface, interfaces);
            type.SetupGet(m => m.ImportedFrom).Returns(importedFrom);

            return type;
        }

        private static Mock<T> CreateTypeMock<T>(
            string fullName,
            ITypeDefinition baseType,
            IAssembly assembly,            
            IAssembly forwardedFrom,
            bool isInterface,
            IEnumerable<ITypeDefinition> interfaces)
            where T : class, ITypeDefinition
        {
            var type = new Mock<T>();

            type.SetupGet<TypeName>(p => p.Name).Returns(new TypeName(It.IsAny<ITypeDefinition>()) { FullName = fullName });
            type.Setup(m => m.Equals(It.IsAny<ITypeDefinition>())).Returns<ITypeDefinition>((other) => string.Equals(other.Name.FullName, fullName));
            type.SetupGet(m => m.Assembly).Returns(assembly);
            type.SetupGet(m => m.BaseType).Returns(baseType);
            type.SetupGet(m => m.ForwardedFrom).Returns(forwardedFrom);
            type.SetupGet(m => m.IsInterface).Returns(isInterface);
            type.SetupGet(m => m.Interfaces).Returns(interfaces ?? Enumerable.Empty<ITypeDefinition>());

            return type;
        }    
    }
}
