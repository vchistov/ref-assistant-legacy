using System;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Cache;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.Algorithms.Strategies;
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
            IAssembly interfaceAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;

            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            ITypeDefinition inputClassObj = MockUtils.CreateTypeMock("Class_A", 
                inputAssemblyObj,
                interfaces: new []
                    {
                        MockUtils.CreateTypeMock("Interface_A", interfaceAssemblyObj).Object,
                        MockUtils.CreateTypeMock("Interface_B", interfaceAssemblyObj).Object,
                        MockUtils.CreateTypeMock("Interface_C", interfaceAssemblyObj).Object
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
            IAssembly firstAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly secondAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;

            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_III").Object;
            ITypeDefinition inputClassObj = MockUtils.CreateTypeMock("Class_A",
                inputAssemblyObj,
                interfaces: new[]
                    {
                        MockUtils.CreateTypeMock("Interface_A", firstAssemblyObj).Object,
                        MockUtils.CreateTypeMock("Interface_B", secondAssemblyObj).Object
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
            IAssembly interfaceAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly interfaceForwardedAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;

            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_III").Object;
            ITypeDefinition inputClassObj = MockUtils.CreateTypeMock("Class_A",
                inputAssemblyObj,
                interfaces: new[]
                    {
                        MockUtils.CreateTypeMock("Interface_A", interfaceAssemblyObj).Object,
                        MockUtils.CreateTypeMock("Interface_B", interfaceAssemblyObj, interfaceForwardedAssemblyObj).Object
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
            IAssembly interfaceAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;

            IAssembly inputForwardedAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_III").Object;
            ITypeDefinition inputClassObj = MockUtils.CreateTypeMock("Class_A",
                inputAssemblyObj,
                forwardedFrom: inputForwardedAssemblyObj,
                interfaces: new[]
                    {
                        MockUtils.CreateTypeMock("Interface_A", interfaceAssemblyObj).Object
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
            IAssembly interfaceAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            ITypeDefinition interfaceObj = MockUtils.CreateTypeMock("Interface_A", interfaceAssemblyObj).Object;

            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            ITypeDefinition inputClassObj = MockUtils.CreateTypeMock("Class_A",
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
            IAssembly interfaceAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            ITypeDefinition interfaceObj = MockUtils.CreateTypeMock("Interface_A", interfaceAssemblyObj).Object;

            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            ITypeDefinition inputClassObj = MockUtils.CreateTypeMock("Class_A",
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
            IAssembly interfaceAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly interfaceImportedAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            IAssembly interfaceDefinitionImportedAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_III").Object;

            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_IV").Object;
            ITypeDefinition inputClassObj = MockUtils.CreateTypeMock("Class_A",
                inputAssemblyObj,
                interfaces: new[] 
                    {
                        MockUtils.CreateTypeMock("Interface_A", interfaceAssemblyObj).Object,
                        MockUtils.CreateTypeMock("Interface_B", interfaceDefinitionImportedAssemblyObj, interfaceImportedAssemblyObj).Object
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
            IAssembly interfaceAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly interfaceDefinitionImportedAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;

            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_III").Object;
            ITypeDefinition inputClassObj = MockUtils.CreateTypeMock("Class_A",
                inputAssemblyObj,
                interfaces: new[] 
                    {
                        MockUtils.CreateTypeMock("Interface_A", interfaceAssemblyObj).Object,
                        MockUtils.CreateTypeMock("Interface_B", interfaceDefinitionImportedAssemblyObj, (IAssembly)null).Object
                    }).Object;

            var result = new TypeInterfacesStrategy(new UsedTypesCache())
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.That(result, Is.EquivalentTo(new[] { inputAssemblyObj, interfaceAssemblyObj, interfaceDefinitionImportedAssemblyObj }));
        }
    }
}
