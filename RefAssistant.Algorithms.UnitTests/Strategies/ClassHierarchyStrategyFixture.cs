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
    public class ClassHierarchyStrategyFixture
    {
        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void Ctor_UsedTypesCacheIsNull_Exception()
        {
            new ClassHierarchyStrategy(null);
        }

        [Test]
        public void DoAnalysis_InputClassIsNull_EmptyResult()
        {
            var result = new ClassHierarchyStrategy(new UsedTypesCache())
                .DoAnalysis(null)
                .ToList();

            Assert.IsNotNull(result);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void DoAnalysis_NoBaseClass_ReturnsAssemblyOfInputClass()
        {
            IAssembly inputAssemblyObj = CreateAssemblyMock("Assembly_I").Object;
            IType inputClassObj = CreateTypeMock("Class_A", inputAssemblyObj).Object;

            var result = new ClassHierarchyStrategy(new UsedTypesCache())
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.That(result, Is.EquivalentTo(Enumerable.Repeat(inputAssemblyObj, 1)));
        }

        [Test]
        public void DoAnalysis_NoBaseClassAndPassedClassIsCached_EmptyResult()
        {
            IType inputClassObj = CreateTypeMock("Class_A", CreateAssemblyMock("Assembly_I").Object).Object;

            var cache = new UsedTypesCache();
            cache.AddType(inputClassObj);

            var result = new ClassHierarchyStrategy(cache)
                .DoAnalysis(inputClassObj)
                .ToList();
            
            Assert.IsNotNull(result);
            Assert.That(result, Is.Empty);
        }
        
        [Test]
        public void DoAnalysis_BaseClassIsCached_ReturnsAssemblyOfInputClass()
        {
            IType baseClassObj = CreateTypeMock("Class_B", CreateAssemblyMock("Assemply_II").Object).Object;
            
            var inputAssemblyObj = CreateAssemblyMock("Assembly_I").Object;
            IType inputClassObj = CreateTypeMock("Class_A", inputAssemblyObj, baseClassObj).Object;

            var cache = new UsedTypesCache();
            cache.AddType(baseClassObj);

            var result = new ClassHierarchyStrategy(cache)
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.That(result, Is.EquivalentTo(Enumerable.Repeat(inputAssemblyObj, 1)));
        }

        [Test]
        public void DoAnalysis_HierarchyIsCached_EmptyResult()
        {
            IType rootClassObj = CreateTypeMock("Class_C", CreateAssemblyMock("Assembly_C").Object).Object;
            IType baseClassObj = CreateTypeMock("Class_B", CreateAssemblyMock("Assembly_B").Object, rootClassObj).Object;
            IType inputClassObj = CreateTypeMock("Class_A", CreateAssemblyMock("Assembly_A").Object, baseClassObj).Object;

            var cache = new UsedTypesCache();
            cache.AddType(rootClassObj);
            cache.AddType(baseClassObj);
            cache.AddType(inputClassObj);
            
            var result = new ClassHierarchyStrategy(cache)
                .DoAnalysis(inputClassObj)
                .ToList();
            
            Assert.IsNotNull(result);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void DoAnalysis_HierarchyIsNotCached_NoDuplicatedAssembliesInResult()
        {
            IAssembly parentAssemblyObj = CreateAssemblyMock("Assembly_II").Object;

            IType rootClassObj = CreateTypeMock("Class_C", parentAssemblyObj).Object;
            IType baseClassObj = CreateTypeMock("Class_B", parentAssemblyObj, rootClassObj).Object;

            IAssembly inputAssemblyObj = CreateAssemblyMock("Assembly_I").Object;
            IType inputClassObj = CreateTypeMock("Class_A", inputAssemblyObj, baseClassObj).Object;
            
            var result = new ClassHierarchyStrategy(new UsedTypesCache())
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.That(result, Is.EquivalentTo(new[] { parentAssemblyObj, inputAssemblyObj }));
        }

        [Test]
        public void DoAnalysis_BaseClassIsForwarded_TwoAssemblyForBaseClassInResult()
        {
            IAssembly forwardedAssemblyObj = CreateAssemblyMock("Assembly_III").Object;
            IAssembly parentAssemblyObj = CreateAssemblyMock("Assembly_II").Object;

            Mock<IType> baseClass = CreateTypeMock("Class_B", parentAssemblyObj);
            baseClass.SetupGet<IAssembly>(p => p.ForwardedFrom).Returns(forwardedAssemblyObj);
            IType baseClassObj = baseClass.Object;

            IAssembly inputAssemblyObj = CreateAssemblyMock("Assembly_I").Object;
            IType inputClassObj = CreateTypeMock("Class_A", inputAssemblyObj, baseClassObj).Object;

            var result = new ClassHierarchyStrategy(new UsedTypesCache())
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            Assert.That(result, Is.EquivalentTo(new[] { parentAssemblyObj, forwardedAssemblyObj, inputAssemblyObj }));
        }

        [Test]
        public void DoAnalysis_BaseClassIsForwardedAndCached_ReturnsAssemblyOfInputClass()
        {
            IAssembly forwardedAssemblyObj = CreateAssemblyMock("Assembly_III").Object;
            IAssembly parentAssemblyObj = CreateAssemblyMock("Assembly_II").Object;

            Mock<IType> baseClass = CreateTypeMock("Class_B", parentAssemblyObj);
            baseClass.SetupGet<IAssembly>(p => p.ForwardedFrom).Returns(forwardedAssemblyObj);
            IType baseClassObj = baseClass.Object;

            IAssembly inputAssemblyObj = CreateAssemblyMock("Assembly_I").Object;
            IType inputClassObj = CreateTypeMock("Class_A", inputAssemblyObj, baseClassObj).Object;

            var cache = new UsedTypesCache();
            cache.AddType(baseClassObj);

            var result = new ClassHierarchyStrategy(cache)
                .DoAnalysis(inputClassObj)
                .ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.That(result, Is.EquivalentTo(new[] { inputAssemblyObj }));
        }

        #region Helpers
        
        private static Mock<IType> CreateTypeMock(string fullName, IAssembly assembly, IType baseClass = null)
        {
            var type = new Mock<IType>();

            type.SetupGet<TypeName>(p => p.Name).Returns(new TypeName(It.IsAny<IType>()) { FullName = fullName });
            type.Setup(m => m.Equals(It.IsAny<IType>())).Returns<IType>((other) => string.Equals(other.Name.FullName, fullName));
            type.SetupGet<IAssembly>(p => p.Assembly).Returns(assembly);
            type.SetupGet<IType>(p => p.BaseType).Returns(baseClass);

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
