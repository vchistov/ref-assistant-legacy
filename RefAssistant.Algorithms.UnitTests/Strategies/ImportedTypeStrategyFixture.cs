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
            ITypeImport inputClassObj = MockUtils.CreateTypeMock(
                "Class_A",
                null,
                MockUtils.CreateAssemblyMock("Assembly_I").Object).Object;

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
            IAssembly importedFromObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            ITypeImport inputClassObj = MockUtils.CreateTypeMock("Class_A", null, importedFromObj).Object;

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
            ITypeImport inputClassObj = MockUtils.CreateTypeMock("Class_A", null, (IAssembly)null).Object;

            new ImportedTypeStrategy(new UsedTypesCache())
                .DoAnalysis(inputClassObj)
                .ToList();
        }
    }
}
