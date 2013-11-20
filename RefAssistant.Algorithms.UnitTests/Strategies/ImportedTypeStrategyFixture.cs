using System;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Cache;
using Lardite.RefAssistant.Algorithms.Data;
using Lardite.RefAssistant.Algorithms.Strategies;
using Moq;
using NUnit.Framework;

namespace Lardite.RefAssistant.Algorithms.UnitTests.Strategies
{
    /// <remarks>
    /// Class A is a primary class, Class B is a parent, Class C is a grandparent and so on.
    /// </remarks>
    [TestFixture]
    public class ImportedTypeStrategyFixture
    {
        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void Ctor_UsedTypesCacheIsNull_Exception()
        {
            new ImportedTypeStrategy(null);
        }

        [Test]
        public void DoAnalysis_InputClassIsNull_EmptyResult()
        {
            var result = new ImportedTypeStrategy(new UsedTypesCache())
                .DoAnalysis(null)
                .ToList();

            Assert.IsNotNull(result);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void DoAnalysis_InputClassIsCached_EmptyResult()
        {
            ITypeImport inputClassObj = CreateTypeMock("Class_A", CreateAssemblyMock("Assembly_I").Object).Object;

            var cache = new UsedTypesCache();
            cache.AddType(inputClassObj);

            var result = new ImportedTypeStrategy(cache)
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void DoAnalysis_InputClassIsNotCached_ReturnsImportedFromAssembly()
        {
            IAssembly importedFromObj = CreateAssemblyMock("Assembly_I").Object;
            ITypeImport inputClassObj = CreateTypeMock("Class_A", importedFromObj).Object;

            var result = new ImportedTypeStrategy(new UsedTypesCache())
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.That(result, Is.EquivalentTo(new[] { importedFromObj }));
        }

        [Test]
        [ExpectedException(ExpectedExceptionName = "System.Diagnostics.Contracts.__ContractsRuntime+ContractException", ExpectedMessage = "Assertion failed.")]
        public void DoAnalysis_NoImportedFrom_ContractException()
        {
            ITypeImport inputClassObj = CreateTypeMock("Class_A", null).Object;

            new ImportedTypeStrategy(new UsedTypesCache())
                .DoAnalysis(inputClassObj)
                .ToList();
        }

        #region Helpers

        private static Mock<ITypeImport> CreateTypeMock(string fullName, IAssembly importedFrom)
        {
            var type = new Mock<ITypeImport>();

            type.SetupGet<TypeName>(p => p.Name).Returns(new TypeName(It.IsAny<ITypeDefinition>()) { FullName = fullName });
            type.Setup(m => m.Equals(It.IsAny<ITypeDefinition>())).Returns<ITypeDefinition>((other) => string.Equals(other.Name.FullName, fullName));
            type.SetupGet<IAssembly>(p => p.ImportedFrom).Returns(importedFrom);

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
