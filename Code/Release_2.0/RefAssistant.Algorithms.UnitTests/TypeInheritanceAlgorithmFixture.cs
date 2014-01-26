using System.Linq;
using Lardite.RefAssistant.Algorithms.Cache;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.Algorithms.Strategies;
using Moq;
using NUnit.Framework;

namespace Lardite.RefAssistant.Algorithms.UnitTests
{
    [TestFixture]
    public class TypeInheritanceAlgorithmFixture
    {
        [TestCase(true)]
        [TestCase(false)]
        public void Process_VerifyCallingStrategies_AllStrategiesAreCalled(bool isInterface)
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            ITypeDefinition inputClassObj = MockUtils.CreateTypeMock(
                "Class_A", 
                inputAssemblyObj,
                isInterface: isInterface).Object;

            var classStrategyMock = new Mock<IStrategy<ITypeDefinition>>();
            classStrategyMock
                .Setup(m => m.DoAnalysis(inputClassObj))
                .Returns(Enumerable.Repeat<IAssembly>(inputAssemblyObj,1));

            var interfaceStrategyMock = new Mock<IStrategy<ITypeDefinition>>();
            interfaceStrategyMock
                .Setup(m => m.DoAnalysis(inputClassObj))
                .Returns(Enumerable.Repeat<IAssembly>(inputAssemblyObj, 1));

            var algorithm = new TypeInheritanceAlgorithm(
                classStrategyMock.Object,
                interfaceStrategyMock.Object);

            var result = algorithm.Process(Enumerable.Repeat<ITypeDefinition>(inputClassObj, 1));

            classStrategyMock.Verify(m => m.DoAnalysis(inputClassObj), Times.Once);
            interfaceStrategyMock.Verify(m => m.DoAnalysis(inputClassObj), Times.Once);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.RequiredFor.Count);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        [Test]
        public void Process_VerifyCallingStrategiesForImported_AllStrategiesAreCalled()
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly inputImportedFromObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            ITypeDefinition inputClassObj = MockUtils.CreateTypeMock(
                "Class_A",
                inputAssemblyObj,
                importedFrom: inputImportedFromObj).Object;

            var classStrategyMock = new Mock<IStrategy<ITypeDefinition>>();
            classStrategyMock
                .Setup(m => m.DoAnalysis(inputClassObj))
                .Returns(new [] { inputAssemblyObj, inputImportedFromObj });

            var interfaceStrategyMock = new Mock<IStrategy<ITypeDefinition>>();
            interfaceStrategyMock
                .Setup(m => m.DoAnalysis(inputClassObj))
                .Returns(Enumerable.Repeat<IAssembly>(inputAssemblyObj, 1));

            var algorithm = new TypeInheritanceAlgorithm(
                classStrategyMock.Object,
                interfaceStrategyMock.Object);

            var result = algorithm.Process(Enumerable.Repeat<ITypeDefinition>(inputClassObj, 1));

            classStrategyMock.Verify(m => m.DoAnalysis(inputClassObj), Times.Once);
            interfaceStrategyMock.Verify(m => m.DoAnalysis(inputClassObj), Times.Once);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.RequiredFor.Count);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        [Test]
        public void Process_Interface5AInCache_DoesNotReturnAssemblyOfInterface5A()
        {
            #region Codeuml
            /*
package "Assembly_III" {
    interface Interface_III_A    
}

package "Assembly_IV" {
    interface Interface_IV_A
}

package "Assembly_V" {
    interface Interface_V_A
}

package "Assembly_II" {
    class Class_II_A    
    Interface_III_A <|- Class_II_A
    Interface_IV_A <|- Class_II_A
}

package "Assembly_VI" {
    interface Interface_VI_A
}

package "Assembly_VII" {
    interface Interface_VII_A
    Interface_VI_A <|- Interface_VII_A
    
    interface Interface_VII_B
}

package "Assembly_VIII" {
    interface Interface_VIII_A
    interface Interface_VIII_B
    interface Interface_VIII_C
    
    Interface_VIII_C <|- Interface_VIII_A
    Interface_VIII_B <|- Interface_VIII_A
}

package "Assembly_I" {
    interface Imp_Interface_V_A
    Interface_V_A - Imp_Interface_V_A : import
    
    interface Imp_Interface_VII_A
    Interface_VII_A - Imp_Interface_VII_A : import
    
    interface Imp_Interface_VII_B
    Interface_VII_B - Imp_Interface_VII_B : import
    
    interface Imp_Interface_VI_A
    Interface_VI_A - Imp_Interface_VI_A : import
    Imp_Interface_VI_A <|- Imp_Interface_VII_A
    
    class Class_I_A
    Class_II_A <|- Class_I_A
    
    class Class_I_B 
    Imp_Interface_V_A <|- Class_I_B
    
    interface Interface_I_A
    Imp_Interface_VII_A <|- Interface_I_A
    Imp_Interface_VII_B <|- Interface_I_A
    
    class Class_I_C 
    Interface_VIII_A <|- Class_I_C
} */

            #endregion

            #region Given

            var assembly3Obj = MockUtils.CreateAssemblyMock("Assembly_III").Object;
            var interface3AObj = MockUtils.CreateTypeMock("Interface_III_A", assembly3Obj, isInterface: true).Object;

            var assembly4Obj = MockUtils.CreateAssemblyMock("Assembly_IV").Object;
            var interface4AObj = MockUtils.CreateTypeMock("Interface_IV_A", assembly4Obj, isInterface: true).Object;

            var assembly5Obj = MockUtils.CreateAssemblyMock("Assembly_V").Object;

            var assembly2Obj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            var class2AObj = MockUtils.CreateTypeMock("Class_II_A", assembly2Obj, interfaces: new[] { interface3AObj, interface4AObj }).Object;

            var assembly6Obj = MockUtils.CreateAssemblyMock("Assembly_VI").Object;

            var assembly7Obj = MockUtils.CreateAssemblyMock("Assembly_VII").Object;

            var assembly8Obj = MockUtils.CreateAssemblyMock("Assembly_VIII").Object;
            var interface8BObj = MockUtils.CreateTypeMock("Interface_VIII_B", assembly8Obj, isInterface: true).Object;
            var interface8CObj = MockUtils.CreateTypeMock("Interface_VIII_C", assembly8Obj, isInterface: true).Object;
            var interface8AObj = MockUtils.CreateTypeMock("Interface_VIII_A", assembly8Obj, isInterface: true, interfaces: new[] { interface8BObj, interface8CObj }).Object;

            var inputAssembly1Obj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            var inputImpInterface5AObj = MockUtils.CreateTypeMock("Interface_V_A", inputAssembly1Obj, assembly5Obj, isInterface: true).Object;
            var inputClass1AObj = MockUtils.CreateTypeMock("Class_I_A", inputAssembly1Obj, baseType: class2AObj).Object;
            var inputClass1BObj = MockUtils.CreateTypeMock("Class_I_B", inputAssembly1Obj, interfaces: new[] { inputImpInterface5AObj }).Object;
            var inputClass1CObj = MockUtils.CreateTypeMock("Class_I_C", inputAssembly1Obj, interfaces: new[] { interface8AObj, interface8BObj, interface8CObj }).Object;
            var inputImpInterface6AObj = MockUtils.CreateTypeMock("Interface_VI_A", inputAssembly1Obj, assembly6Obj, isInterface: true).Object;
            var inputImpInterface7AObj = MockUtils.CreateTypeMock("Interface_VII_A", inputAssembly1Obj, assembly7Obj, isInterface: true, interfaces: new[] { inputImpInterface6AObj }).Object;
            var inputImpInterface7BObj = MockUtils.CreateTypeMock("Interface_VII_B", inputAssembly1Obj, assembly7Obj, isInterface: true).Object;
            var inputInterface1AObj = MockUtils.CreateTypeMock("Interface_I_A", inputAssembly1Obj, isInterface: true, interfaces: new[] { inputImpInterface7AObj, inputImpInterface6AObj, inputImpInterface7BObj }).Object;

            #endregion

            var cache = new UsedTypesCache();
            cache.AddType(inputImpInterface5AObj);

            var algorithm = new TypeInheritanceAlgorithm(
                new ClassHierarchyStrategy(cache),
                new TypeInterfacesStrategy(cache));

            var result = algorithm.Process(new[] 
                { 
                    inputClass1AObj, 
                    inputClass1BObj,
                    inputClass1CObj,
                    inputInterface1AObj,
                    inputImpInterface5AObj,
                    inputImpInterface6AObj,
                    inputImpInterface7AObj,
                    inputImpInterface7BObj
                });

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.RequiredFor.Count);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
            CollectionAssert.AreEquivalent(new[] 
                { 
                    inputAssembly1Obj,
                    assembly2Obj,
                    assembly6Obj,
                    assembly7Obj,
                    assembly8Obj
                },
                result.RequiredFor);
        }

        [Test]
        public void Process_EmptyCache_ReturnsSixAssemblies()
        {
            #region Codeuml
            /*
package "Assembly_III" {
    interface Interface_III_A    
}

package "Assembly_IV" {
    interface Interface_IV_A
}

package "Assembly_V" {
    interface Interface_V_A
}

package "Assembly_II" {
    class Class_II_A    
    Interface_III_A <|- Class_II_A
    Interface_IV_A <|- Class_II_A
}

package "Assembly_VI" {
    interface Interface_VI_A
}

package "Assembly_VII" {
    interface Interface_VII_A
    Interface_VI_A <|- Interface_VII_A
    
    interface Interface_VII_B
}

package "Assembly_VIII" {
    interface Interface_VIII_A
    interface Interface_VIII_B
    interface Interface_VIII_C
    
    Interface_VIII_C <|- Interface_VIII_A
    Interface_VIII_B <|- Interface_VIII_A
}

package "Assembly_I" {
    interface Imp_Interface_V_A
    Interface_V_A - Imp_Interface_V_A : import
    
    interface Imp_Interface_VII_A
    Interface_VII_A - Imp_Interface_VII_A : import
    
    interface Imp_Interface_VII_B
    Interface_VII_B - Imp_Interface_VII_B : import
    
    interface Imp_Interface_VI_A
    Interface_VI_A - Imp_Interface_VI_A : import
    Imp_Interface_VI_A <|- Imp_Interface_VII_A
    
    class Class_I_A
    Class_II_A <|- Class_I_A
    
    class Class_I_B 
    Imp_Interface_V_A <|- Class_I_B
    
    interface Interface_I_A
    Imp_Interface_VII_A <|- Interface_I_A
    Imp_Interface_VII_B <|- Interface_I_A
    
    class Class_I_C 
    Interface_VIII_A <|- Class_I_C
} */

            #endregion

            #region Given

            var assembly3Obj = MockUtils.CreateAssemblyMock("Assembly_III").Object;
            var interface3AObj = MockUtils.CreateTypeMock("Interface_III_A", assembly3Obj, isInterface: true).Object;

            var assembly4Obj = MockUtils.CreateAssemblyMock("Assembly_IV").Object;
            var interface4AObj = MockUtils.CreateTypeMock("Interface_IV_A", assembly4Obj, isInterface: true).Object;

            var assembly5Obj = MockUtils.CreateAssemblyMock("Assembly_V").Object;

            var assembly2Obj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            var class2AObj = MockUtils.CreateTypeMock("Class_II_A", assembly2Obj, interfaces: new[] { interface3AObj, interface4AObj }).Object;

            var assembly6Obj = MockUtils.CreateAssemblyMock("Assembly_VI").Object;

            var assembly7Obj = MockUtils.CreateAssemblyMock("Assembly_VII").Object;

            var assembly8Obj = MockUtils.CreateAssemblyMock("Assembly_VIII").Object;
            var interface8BObj = MockUtils.CreateTypeMock("Interface_VIII_B", assembly8Obj, isInterface: true).Object;
            var interface8CObj = MockUtils.CreateTypeMock("Interface_VIII_C", assembly8Obj, isInterface: true).Object;
            var interface8AObj = MockUtils.CreateTypeMock("Interface_VIII_A", assembly8Obj, isInterface: true, interfaces: new[] { interface8BObj, interface8CObj }).Object;

            var inputAssembly1Obj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            var inputImpInterface5AObj = MockUtils.CreateTypeMock("Interface_V_A", inputAssembly1Obj, assembly5Obj, isInterface: true).Object;
            var inputClass1AObj = MockUtils.CreateTypeMock("Class_I_A", inputAssembly1Obj, baseType: class2AObj).Object;
            var inputClass1BObj = MockUtils.CreateTypeMock("Class_I_B", inputAssembly1Obj, interfaces: new[] { inputImpInterface5AObj }).Object;
            var inputClass1CObj = MockUtils.CreateTypeMock("Class_I_C", inputAssembly1Obj, interfaces: new[] { interface8AObj, interface8BObj, interface8CObj }).Object;
            var inputImpInterface6AObj = MockUtils.CreateTypeMock("Interface_VI_A", inputAssembly1Obj, assembly6Obj, isInterface: true).Object;
            var inputImpInterface7AObj = MockUtils.CreateTypeMock("Interface_VII_A", inputAssembly1Obj, assembly7Obj, isInterface: true, interfaces: new[] { inputImpInterface6AObj }).Object;
            var inputImpInterface7BObj = MockUtils.CreateTypeMock("Interface_VII_B", inputAssembly1Obj, assembly7Obj, isInterface: true).Object;
            var inputInterface1AObj = MockUtils.CreateTypeMock("Interface_I_A", inputAssembly1Obj, isInterface: true, interfaces: new[] { inputImpInterface7AObj, inputImpInterface6AObj, inputImpInterface7BObj }).Object;

            #endregion

            var cache = new UsedTypesCache();

            var algorithm = new TypeInheritanceAlgorithm(
                new ClassHierarchyStrategy(cache),
                new TypeInterfacesStrategy(cache));

            var result = algorithm.Process(new[] 
                { 
                    inputClass1AObj, 
                    inputClass1BObj,
                    inputClass1CObj,
                    inputInterface1AObj,
                    inputImpInterface5AObj,
                    inputImpInterface6AObj,
                    inputImpInterface7AObj,
                    inputImpInterface7BObj
                });

            Assert.IsNotNull(result);
            Assert.AreEqual(6, result.RequiredFor.Count);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
            CollectionAssert.AreEquivalent(new[] 
                { 
                    inputAssembly1Obj,
                    assembly2Obj,
                    assembly5Obj,
                    assembly6Obj,
                    assembly7Obj,
                    assembly8Obj
                },
                result.RequiredFor);
        }
    }
}
