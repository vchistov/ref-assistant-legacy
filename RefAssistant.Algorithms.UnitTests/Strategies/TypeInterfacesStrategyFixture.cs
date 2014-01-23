using System;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Cache;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.Algorithms.Strategies;
using Moq;
using NUnit.Framework;

namespace Lardite.RefAssistant.Algorithms.UnitTests.Strategies
{
    /// <remarks>
    /// Class A is a primary class, Class B is a parent, Class C is a grandparent and so on.
    /// </remarks>
    [TestFixture]
    public class TypeInterfacesStrategyFixture
    {
        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void Ctor_UsedTypesCacheIsNull_Exception()
        {
            new TypeInterfacesStrategy(null);
        }

        [Test]
        public void DoAnalysis_InputClassIsNull_EmptyResult()
        {
            var result = new TypeInterfacesStrategy(new UsedTypesCache())
                .DoAnalysis(null)
                .ToList();

            Assert.IsNotNull(result);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void DoAnalysis_AllInterfacesFromSingleAssembly_ReturnsTwoAssemblies()
        {
            IAssembly interfaceAssemblyObj = CreateAssemblyMock("Assembly_I").Object;

            IAssembly inputAssemblyObj = CreateAssemblyMock("Assembly_II").Object;
            ITypeDefinition inputClassObj = CreateTypeMock("Class_A", 
                inputAssemblyObj,
                interfaces: new []
                    {
                        CreateTypeMock("Interface_A", interfaceAssemblyObj).Object,
                        CreateTypeMock("Interface_B", interfaceAssemblyObj).Object,
                        CreateTypeMock("Interface_C", interfaceAssemblyObj).Object
                    }).Object;

            var result = new TypeInterfacesStrategy(new UsedTypesCache())
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.That(result, Is.EquivalentTo(new[] { interfaceAssemblyObj, inputAssemblyObj }));
        }

        [Test]
        public void DoAnalysis_AllInterfacesFromDifferentAssemblies_ReturnsAllAssemblies()
        {
            IAssembly firstAssemblyObj = CreateAssemblyMock("Assembly_I").Object;
            IAssembly secondAssemblyObj = CreateAssemblyMock("Assembly_II").Object;

            IAssembly inputAssemblyObj = CreateAssemblyMock("Assembly_III").Object;
            ITypeDefinition inputClassObj = CreateTypeMock("Class_A",
                inputAssemblyObj,
                interfaces: new[]
                    {
                        CreateTypeMock("Interface_A", firstAssemblyObj).Object,
                        CreateTypeMock("Interface_B", secondAssemblyObj).Object
                    }).Object;

            var result = new TypeInterfacesStrategy(new UsedTypesCache())
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.That(result, Is.EquivalentTo(new[] { firstAssemblyObj, secondAssemblyObj, inputAssemblyObj }));
        }

        [Test]
        public void DoAnalysis_InterfaceIsForwarded_ReturnsTwoAssembliesForForwardedInterface()
        {
            IAssembly interfaceAssemblyObj = CreateAssemblyMock("Assembly_I").Object;
            IAssembly interfaceForwardedAssemblyObj = CreateAssemblyMock("Assembly_II").Object;

            IAssembly inputAssemblyObj = CreateAssemblyMock("Assembly_III").Object;
            ITypeDefinition inputClassObj = CreateTypeMock("Class_A",
                inputAssemblyObj,
                interfaces: new[]
                    {
                        CreateTypeMock("Interface_A", interfaceAssemblyObj).Object,
                        CreateTypeMock("Interface_B", interfaceAssemblyObj, interfaceForwardedAssemblyObj).Object
                    }).Object;

            var result = new TypeInterfacesStrategy(new UsedTypesCache())
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.That(result, Is.EquivalentTo(new[] { interfaceAssemblyObj, interfaceForwardedAssemblyObj, inputAssemblyObj }));
        }

        [Test]
        public void DoAnalysis_InputTypeIsForwarded_ReturnsTwoAssembliesForInputType()
        {
            IAssembly interfaceAssemblyObj = CreateAssemblyMock("Assembly_I").Object;

            IAssembly inputForwardedAssemblyObj = CreateAssemblyMock("Assembly_II").Object;
            IAssembly inputAssemblyObj = CreateAssemblyMock("Assembly_III").Object;
            ITypeDefinition inputClassObj = CreateTypeMock("Class_A",
                inputAssemblyObj,
                inputForwardedAssemblyObj,
                interfaces: new[]
                    {
                        CreateTypeMock("Interface_A", interfaceAssemblyObj).Object
                    }).Object;

            var result = new TypeInterfacesStrategy(new UsedTypesCache())
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.That(result, Is.EquivalentTo(new[] { interfaceAssemblyObj, inputForwardedAssemblyObj, inputAssemblyObj }));
        }

        [Test]
        public void DoAnalysis_InterfaceIsCached_ReturnsAssemblyOfInputType()
        {
            IAssembly interfaceAssemblyObj = CreateAssemblyMock("Assembly_I").Object;
            ITypeDefinition interfaceObj = CreateTypeMock("Interface_A", interfaceAssemblyObj).Object;

            IAssembly inputAssemblyObj = CreateAssemblyMock("Assembly_II").Object;
            ITypeDefinition inputClassObj = CreateTypeMock("Class_A",
                inputAssemblyObj,
                interfaces: new [] 
                    {
                        interfaceObj
                    }).Object;

            var cache = new UsedTypesCache();
            cache.AddType(interfaceObj);

            var result = new TypeInterfacesStrategy(cache)
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.That(result, Is.EquivalentTo(new[] { inputAssemblyObj }));
        }

        [Test]
        public void DoAnalysis_InputIsCached_ReturnsAssemblyOfInputType()
        {
            IAssembly interfaceAssemblyObj = CreateAssemblyMock("Assembly_I").Object;
            ITypeDefinition interfaceObj = CreateTypeMock("Interface_A", interfaceAssemblyObj).Object;

            IAssembly inputAssemblyObj = CreateAssemblyMock("Assembly_II").Object;
            ITypeDefinition inputClassObj = CreateTypeMock("Class_A",
                inputAssemblyObj,
                interfaces: new[] 
                    {
                        interfaceObj
                    }).Object;

            var cache = new UsedTypesCache();
            cache.AddType(interfaceObj);
            cache.AddType(inputClassObj);

            var result = new TypeInterfacesStrategy(cache)
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.That(result, Is.EquivalentTo(new[] { inputAssemblyObj }));
        }

        [Test]
        public void DoAnalysis_InterfaceIsImported_ReturnsTwoAssembliesForImportedInterface()
        {
            IAssembly interfaceAssemblyObj = CreateAssemblyMock("Assembly_I").Object;
            IAssembly interfaceImportedAssemblyObj = CreateAssemblyMock("Assembly_II").Object;
            IAssembly interfaceDefinitionImportedAssemblyObj = CreateAssemblyMock("Assembly_III").Object;

            IAssembly inputAssemblyObj = CreateAssemblyMock("Assembly_IV").Object;
            ITypeDefinition inputClassObj = CreateTypeMock("Class_A",
                inputAssemblyObj,
                interfaces: new[] 
                    {
                        CreateTypeMock("Interface_A", interfaceAssemblyObj).Object,
                        CreateImportedTypeMock("Interface_B", interfaceDefinitionImportedAssemblyObj, interfaceImportedAssemblyObj).Object
                    }).Object;

            var result = new TypeInterfacesStrategy(new UsedTypesCache())
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
            Assert.That(result, Is.EquivalentTo(new[] { inputAssemblyObj, interfaceAssemblyObj, interfaceDefinitionImportedAssemblyObj, interfaceImportedAssemblyObj }));
        }

        [Test]
        public void DoAnalysis_InterfaceIsImported_ReturnsOnlyAssemblyWhereTypeIsImported()
        {
            IAssembly interfaceAssemblyObj = CreateAssemblyMock("Assembly_I").Object;
            IAssembly interfaceDefinitionImportedAssemblyObj = CreateAssemblyMock("Assembly_II").Object;

            IAssembly inputAssemblyObj = CreateAssemblyMock("Assembly_III").Object;
            ITypeDefinition inputClassObj = CreateTypeMock("Class_A",
                inputAssemblyObj,
                interfaces: new[] 
                    {
                        CreateTypeMock("Interface_A", interfaceAssemblyObj).Object,
                        CreateImportedTypeMock("Interface_B", interfaceDefinitionImportedAssemblyObj, null).Object
                    }).Object;

            var result = new TypeInterfacesStrategy(new UsedTypesCache())
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.That(result, Is.EquivalentTo(new[] { inputAssemblyObj, interfaceAssemblyObj, interfaceDefinitionImportedAssemblyObj }));
        }
        
        #region Helpers

        private static Mock<ITypeDefinition> CreateTypeMock(string fullName, IAssembly assembly, IAssembly forwardedFrom = null, params ITypeDefinition[] interfaces)
        {
            var type = new Mock<ITypeDefinition>();

            type.SetupGet<TypeName>(p => p.Name).Returns(new TypeName(It.IsAny<ITypeDefinition>()) { FullName = fullName });
            type.Setup(m => m.Equals(It.IsAny<ITypeDefinition>())).Returns<ITypeDefinition>((other) => string.Equals(other.Name.FullName, fullName));
            type.SetupGet(m => m.Assembly).Returns(assembly);
            type.SetupGet(m => m.ForwardedFrom).Returns(forwardedFrom);
            type.SetupGet(m => m.Interfaces).Returns(interfaces ?? Enumerable.Empty<ITypeDefinition>());
            
            return type;
        }

        private static Mock<ITypeImport> CreateImportedTypeMock(string fullName, IAssembly assembly, IAssembly importedFrom, params ITypeDefinition[] interfaces)
        {
            var type = new Mock<ITypeImport>();

            type.SetupGet<TypeName>(p => p.Name).Returns(new TypeName(It.IsAny<ITypeDefinition>()) { FullName = fullName });
            type.Setup(m => m.Equals(It.IsAny<ITypeDefinition>())).Returns<ITypeDefinition>((other) => string.Equals(other.Name.FullName, fullName));
            type.SetupGet(m => m.Assembly).Returns(assembly);
            type.SetupGet(m => m.ImportedFrom).Returns(importedFrom);
            type.SetupGet(m => m.Interfaces).Returns(interfaces ?? Enumerable.Empty<ITypeDefinition>());

            return type;
        }

        private static Mock<IAssembly> CreateAssemblyMock(string name)
        {
            var assembly = new Mock<IAssembly>();

            assembly.SetupGet<string>(p => p.Name).Returns(name);
            assembly.Setup(m => m.Equals(It.IsAny<IAssembly>())).Returns<IAssembly>((other) => string.Equals(other.Name, name));

            return assembly;
        }

        #endregion
    }
}
