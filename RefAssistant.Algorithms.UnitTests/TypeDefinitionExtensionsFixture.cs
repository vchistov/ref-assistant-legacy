using System;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Contracts;
using Moq;
using NUnit.Framework;

namespace Lardite.RefAssistant.Algorithms.UnitTests
{
    [TestFixture]
    public class TypeDefinitionExtensionsFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetMethodWithOverloads_ThisIsNull_ArgumentNullException()
        {
            ITypeDefinition inputTypeDefObj = null;
            Mock<IMethod> inputMethodMock = MockUtils.CreateMethodMock("Method_1");

            inputTypeDefObj.GetMethodWithOverloads(inputMethodMock.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetMethodWithOverloads_MethodIsNull_ArgumentNullException()
        {
            ITypeDefinition inputTypeDefObj = MockUtils.CreateTypeMock(
                "Class_A", 
                MockUtils.CreateAssemblyMock("Assembly_I").Object).Object;

            inputTypeDefObj.GetMethodWithOverloads(null);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetMethodWithOverloads_NoMethods_ReturnsEmpty(bool returnsNull)
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;

            Mock<ITypeDefinition> inputTypeDefMock = MockUtils.CreateTypeMock("Class_A", inputAssemblyObj);
            inputTypeDefMock
                .SetupGet(t => t.Methods)
                .Returns(returnsNull ? null : Enumerable.Empty<IMethod>());

            Mock<IMethod> inputMethodMock = MockUtils.CreateMethodMock("Method_1");

            var result = inputTypeDefMock.Object.GetMethodWithOverloads(inputMethodMock.Object);

            inputTypeDefMock.VerifyGet(t => t.Methods, Times.Once);
            inputMethodMock.VerifyGet(m => m.Parameters, Times.Never);

            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public void GetMethodWithOverloads_MethodNotFound_ReturnsEmpty()
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IMethod inputMethod11AObj = MockUtils.CreateMethodMock(
                "Method_1",
                parameters: new[]
                    {
                        MockUtils.CreateMemberTypeMock(
                            MockUtils.CreateTypeMock(
                                "Method_1_Param1of1_Class_A",
                                MockUtils.CreateAssemblyMock("Assembly_II").Object).Object).Object
                    }).Object;

            IMethod inputMethod12AObj = MockUtils.CreateMethodMock(
                "Method_1",
                parameters: new[]
                    {
                        MockUtils.CreateMemberTypeMock(
                            MockUtils.CreateTypeMock(
                                "Method_1_Param1of2_Class_A", 
                                MockUtils.CreateAssemblyMock("Assembly_II").Object).Object).Object,

                        MockUtils.CreateMemberTypeMock(
                            MockUtils.CreateTypeMock(
                                "Method_1_Param2of2_Class_B",
                                MockUtils.CreateAssemblyMock("Assembly_II").Object).Object).Object
                    }).Object;

            Mock<ITypeDefinition> inputTypeDefMock = MockUtils.CreateTypeMock("Class_A", inputAssemblyObj);
            inputTypeDefMock
                .SetupGet(t => t.Methods)
                .Returns(new[] { inputMethod12AObj });

            var result = inputTypeDefMock.Object.GetMethodWithOverloads(inputMethod11AObj).ToArray();

            inputTypeDefMock.VerifyGet(t => t.Methods, Times.Exactly(2));

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetMethodWithOverloads_ThereAreOverloads_ReturnsSelfAndOverload()
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IMethod inputMethod11AObj = MockUtils.CreateMethodMock(
                "Method_1",
                parameters: new[]
                    {
                        MockUtils.CreateMemberTypeMock(
                            MockUtils.CreateTypeMock(
                                "Method_1_Param1of1_Class_A",
                                MockUtils.CreateAssemblyMock("Assembly_II").Object).Object).Object
                    }).Object;

            IMethod inputMethod11BObj = MockUtils.CreateMethodMock(
                "Method_1",
                parameters: new[]
                    {
                        MockUtils.CreateMemberTypeMock(
                            MockUtils.CreateTypeMock(
                                "Method_1_Param1of1_Class_B",
                                MockUtils.CreateAssemblyMock("Assembly_II").Object).Object).Object
                    }).Object;

            IMethod inputMethod21AObj = MockUtils.CreateMethodMock(
                "Method_2",
                parameters: new[]
                    {
                        MockUtils.CreateMemberTypeMock(
                            MockUtils.CreateTypeMock(
                                "Method_2_Param1of1_Class_A",
                                MockUtils.CreateAssemblyMock("Assembly_II").Object).Object).Object
                    }).Object;

            IMethod inputMethod12AObj = MockUtils.CreateMethodMock(
                "Method_1",
                parameters: new[]
                    {
                        MockUtils.CreateMemberTypeMock(
                            MockUtils.CreateTypeMock(
                                "Method_1_Param1of2_Class_A", 
                                MockUtils.CreateAssemblyMock("Assembly_II").Object).Object).Object,

                        MockUtils.CreateMemberTypeMock(
                            MockUtils.CreateTypeMock(
                                "Method_1_Param2of2_Class_B",
                                MockUtils.CreateAssemblyMock("Assembly_II").Object).Object).Object
                    }).Object;

            Mock<ITypeDefinition> inputTypeDefMock = MockUtils.CreateTypeMock("Class_A", inputAssemblyObj);
            inputTypeDefMock
                .SetupGet(t => t.Methods)
                .Returns(new[]
                    {
                        inputMethod11AObj,
                        inputMethod11BObj,
                        inputMethod21AObj,
                        inputMethod12AObj
                    });

            var result = inputTypeDefMock.Object.GetMethodWithOverloads(inputMethod11AObj).ToArray();

            inputTypeDefMock.VerifyGet(t => t.Methods, Times.Exactly(2));
            
            Assert.That(result, Is.Not.Empty);
            Assert.AreEqual(2, result.Length);
            CollectionAssert.AreEquivalent(new[] 
                {
                    inputMethod11AObj,
                    inputMethod11BObj
                }, 
                result);
        }

        [Test]
        public void GetMethodWithOverloads_NoOverloads_ReturnsSelf()
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IMethod inputMethod11AObj = MockUtils.CreateMethodMock(
                "Method_1",
                parameters: new[]
                    {
                        MockUtils.CreateMemberTypeMock(
                            MockUtils.CreateTypeMock(
                                "Method_1_Param1of1_Class_A",
                                MockUtils.CreateAssemblyMock("Assembly_II").Object).Object).Object
                    }).Object;

            IMethod inputMethod12AObj = MockUtils.CreateMethodMock(
                "Method_1",
                parameters: new[]
                    {
                        MockUtils.CreateMemberTypeMock(
                            MockUtils.CreateTypeMock(
                                "Method_1_Param1of2_Class_A", 
                                MockUtils.CreateAssemblyMock("Assembly_II").Object).Object).Object,

                        MockUtils.CreateMemberTypeMock(
                            MockUtils.CreateTypeMock(
                                "Method_1_Param2of2_Class_B",
                                MockUtils.CreateAssemblyMock("Assembly_II").Object).Object).Object
                    }).Object;

            Mock<ITypeDefinition> inputTypeDefMock = MockUtils.CreateTypeMock("Class_A", inputAssemblyObj);
            inputTypeDefMock
                .SetupGet(t => t.Methods)
                .Returns(new[]
                    {
                        inputMethod11AObj,
                        inputMethod12AObj
                    });

            var result = inputTypeDefMock.Object.GetMethodWithOverloads(inputMethod11AObj).ToArray();

            inputTypeDefMock.VerifyGet(t => t.Methods, Times.Exactly(2));

            Assert.That(result, Is.Not.Empty);
            Assert.AreEqual(1, result.Length);
            CollectionAssert.AreEquivalent(new[] { inputMethod11AObj }, result);
        }
    }
}
