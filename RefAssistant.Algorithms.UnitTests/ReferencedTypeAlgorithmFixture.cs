using System;
using System.Collections.Generic;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Cache;
using Lardite.RefAssistant.Algorithms.Contracts;
using Lardite.RefAssistant.Algorithms.Strategies;
using Moq;
using NUnit.Framework;

namespace Lardite.RefAssistant.Algorithms.UnitTests
{
    [TestFixture]
    public class ReferencedTypeAlgorithmFixture
    {
        [TestCase(true)]
        [TestCase(false)]
        public void ProcessTypeDefinition_VerifyCallingStrategies_AllStrategiesAreCalled(bool isInterface)
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            ITypeReference inputClassObj = MockUtils.CreateTypeReferenceMock(
                MockUtils.CreateTypeMock(
                    "Class_A",
                    inputAssemblyObj,
                    isInterface: isInterface).Object).Object;

            var cache = new UsedTypesCache();

            var classStrategyMock = new Mock<IStrategy<ITypeDefinition>>();
            classStrategyMock
                .Setup(m => m.DoAnalysis(inputClassObj.TypeDefinition))
                .Returns(Enumerable.Repeat<IAssembly>(inputAssemblyObj, 1));

            var interfaceStrategyMock = new Mock<IStrategy<ITypeDefinition>>();
            interfaceStrategyMock
                .Setup(m => m.DoAnalysis(inputClassObj.TypeDefinition))
                .Returns(Enumerable.Repeat<IAssembly>(inputAssemblyObj, 1));

            var algorithm = new ReferencedTypeAlgorithm(
                classStrategyMock.Object,
                interfaceStrategyMock.Object,
                cache);

            var result = algorithm.Process(Enumerable.Repeat<ITypeReference>(inputClassObj, 1));

            classStrategyMock.Verify(m => m.DoAnalysis(inputClassObj.TypeDefinition), Times.Once);
            interfaceStrategyMock.Verify(m => m.DoAnalysis(inputClassObj.TypeDefinition), Times.Once);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.RequiredFor.Count);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ProcessTypeDefinition_TypeIsCached_StrategiesAreNotCalled(bool isInterface)
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            ITypeReference inputClassObj = MockUtils.CreateTypeReferenceMock(
                MockUtils.CreateTypeMock(
                    "Class_A",
                    inputAssemblyObj,
                    isInterface: isInterface).Object).Object;

            var cache = new UsedTypesCache();
            cache.AddType(inputClassObj.TypeDefinition);

            var classStrategyMock = new Mock<IStrategy<ITypeDefinition>>();
            classStrategyMock
                .Setup(m => m.DoAnalysis(inputClassObj.TypeDefinition))
                .Returns(Enumerable.Repeat<IAssembly>(inputAssemblyObj, 1));

            var interfaceStrategyMock = new Mock<IStrategy<ITypeDefinition>>();
            interfaceStrategyMock
                .Setup(m => m.DoAnalysis(inputClassObj.TypeDefinition))
                .Returns(Enumerable.Repeat<IAssembly>(inputAssemblyObj, 1));

            var algorithm = new ReferencedTypeAlgorithm(
                classStrategyMock.Object,
                interfaceStrategyMock.Object,
                cache);

            var result = algorithm.Process(Enumerable.Repeat<ITypeReference>(inputClassObj, 1));

            classStrategyMock.Verify(m => m.DoAnalysis(inputClassObj.TypeDefinition), Times.Never);
            interfaceStrategyMock.Verify(m => m.DoAnalysis(inputClassObj.TypeDefinition), Times.Never);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.RequiredFor.Count);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        [Test]
        public void ProcessMethodReferences_ReturnTypeIsGenericParameter_NoParams_OmitReturnType()
        {
            var methodTypeMock = MockUtils.CreateMemberTypeMock(isGenericParam: true);
            var method1Obj = MockUtils.CreateMethodMock("Method_1", returnType: methodTypeMock.Object).Object;

            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            Mock<ITypeReference> inputTypeMock = MockUtils
                .CreateTypeReferenceMock(MockUtils.CreateTypeMock("Class_A", inputAssemblyObj).AddMethod(method1Obj).Object)
                .AddMethod(method1Obj);

            var algorithm = CreateAlgorithm(new UsedTypesCache());

            var result = algorithm.Process(Enumerable.Repeat<ITypeReference>(inputTypeMock.Object, 1));

            methodTypeMock.Verify(p => p.IsGenericParameter, Times.Once);
            methodTypeMock.Verify(p => p.TypeDefinition, Times.Never);
            methodTypeMock.Verify(p => p.GenericArguments, Times.Never);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.RequiredFor.Count);
            Assert.That(result.RequiredFor.First(), Is.EqualTo(inputAssemblyObj));
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        [Test]
        public void ProcessMethodReferences_ReturnTypeIsGenericParameter_WithParams_OmitReturnType()
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly methodParam1TypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            IAssembly methodParam2ATypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_III").Object;
            IAssembly methodParam2BTypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_IV").Object;

            var returnTypeMock = MockUtils.CreateMemberTypeMock(isGenericParam: true);

            IMethod method1Obj = MockUtils.CreateMethodMock(
                "Method_1",
                returnType: returnTypeMock.Object,
                parameters: new[]
                    {
                        MockUtils.CreateTypeMock("Method_1_Param_1", methodParam1TypeAsmObj).ToMemberType(),

                        MockUtils.CreateMemberTypeMock(
                            genericArgs: new[]
                                {
                                    MockUtils.CreateTypeMock("Method_1_Param_2_A", methodParam2ATypeAsmObj).ToMemberType(),
                                    MockUtils.CreateTypeMock("Method_1_Param_2_B", methodParam2BTypeAsmObj).ToMemberType()
                                }).Object
                    }).Object;

            Mock<ITypeReference> inputTypeRefMock =
                MockUtils.CreateTypeReferenceMock(
                    MockUtils.CreateTypeMock("Class_A", inputAssemblyObj).AddMethod(method1Obj).Object)
                .AddMethod(method1Obj);

            var algorithm = CreateAlgorithm(new UsedTypesCache());
            var result = algorithm.Process(Enumerable.Repeat(inputTypeRefMock.Object, 1));

            returnTypeMock.Verify(p => p.IsGenericParameter, Times.Once);
            returnTypeMock.Verify(p => p.TypeDefinition, Times.Never);
            returnTypeMock.Verify(p => p.GenericArguments, Times.Never);

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.RequiredFor.Count);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    inputAssemblyObj,
                    methodParam1TypeAsmObj,
                    methodParam2ATypeAsmObj,
                    methodParam2BTypeAsmObj
                },
                result.RequiredFor);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        [Test]
        public void ProcessMethodReferences_ReturnTypeIsGenericParameter_WithOverloads_OmitReturnType()
        {
            var returnTypeMock = MockUtils.CreateMemberTypeMock(isGenericParam: true);

            IMethod method1AObj = MockUtils.CreateMethodMock(
                "Method_1",
                returnType: returnTypeMock.Object,
                parameters: new[]
                    {
                        MockUtils.CreateTypeMock(
                            "Class_B", 
                            MockUtils.CreateAssemblyMock("Assembly_II").Object).ToMemberType()
                    }).Object;

            IMethod method1BObj = MockUtils.CreateMethodMock(
                "Method_1",
                returnType: returnTypeMock.Object,
                parameters: new[]
                    {
                        MockUtils.CreateTypeMock(
                            "Class_C",
                            MockUtils.CreateAssemblyMock("Assembly_III").Object).ToMemberType()
                    }).Object;

            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            Mock<ITypeDefinition> inputTypeDefMock =
                MockUtils.CreateTypeMock("Class_A", inputAssemblyObj)
                .AddMethod(method1AObj)
                .AddMethod(method1BObj);

            Mock<ITypeReference> inputTypeRefMock = 
                MockUtils.CreateTypeReferenceMock(inputTypeDefMock.Object)
                .AddMethod(method1AObj);

            var algorithm = CreateAlgorithm(new UsedTypesCache());
            var result = algorithm.Process(Enumerable.Repeat(inputTypeRefMock.Object, 1));

            returnTypeMock.Verify(p => p.IsGenericParameter, Times.AtLeast(1));
            returnTypeMock.Verify(p => p.TypeDefinition, Times.Never);
            returnTypeMock.Verify(p => p.GenericArguments, Times.Never);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.RequiredFor.Count);
            Assert.That(
                result.RequiredFor,
                Is.EquivalentTo(new[]
                {
                    inputTypeDefMock.Object.Assembly,
                    method1AObj.Parameters.First().ParameterType.TypeDefinition.Assembly,
                    method1BObj.Parameters.First().ParameterType.TypeDefinition.Assembly
                }));            
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        [Test]
        public void ProcessMethodReferences_ReturnTypeIsGenericArgument_NoParams_ReturnTypeInResults()
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly methodType1AsmObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            IAssembly methodType2AsmObj = MockUtils.CreateAssemblyMock("Assembly_III").Object;

            Mock<IMemberType> methodTypeMock = MockUtils.CreateMemberTypeMock(
                genericArgs: new[]
                {
                    MockUtils.CreateTypeMock("MemberType_Arg_1", methodType1AsmObj).ToMemberType(),
                    MockUtils.CreateTypeMock("MemberType_Arg_2", methodType2AsmObj).ToMemberType()
                });

            IMethod methodObj = MockUtils.CreateMethodMock("Method_1", returnType: methodTypeMock.Object).Object;

            Mock<ITypeReference> inputTypeRefMock =
                MockUtils.CreateTypeReferenceMock(
                    MockUtils.CreateTypeMock("Class_A", inputAssemblyObj).AddMethod(methodObj).Object)
                    .AddMethod(methodObj);

            var algorithm = CreateAlgorithm(new UsedTypesCache());
            var result = algorithm.Process(Enumerable.Repeat(inputTypeRefMock.Object, 1));

            methodTypeMock.Verify(p => p.IsGenericParameter, Times.Once);
            methodTypeMock.Verify(p => p.TypeDefinition, Times.AtLeastOnce);
            methodTypeMock.Verify(p => p.GenericArguments, Times.AtLeastOnce);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.RequiredFor.Count);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    inputAssemblyObj,
                    methodType1AsmObj,
                    methodType2AsmObj
                },
                result.RequiredFor);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }
        
        [Test]
        public void ProcessMethodReferences_ReturnTypeIsDefined_NoParams_ReturnTypeInResults()
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly returnTypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;

            Mock<IMemberType> returnTypeMock = MockUtils.CreateMemberTypeMock(
                typeDef: MockUtils.CreateTypeMock("MemberType", returnTypeAsmObj).Object);

            IMethod methodObj = MockUtils.CreateMethodMock("Method_1", returnType: returnTypeMock.Object).Object;

            Mock<ITypeReference> inputTypeRefMock =
                MockUtils.CreateTypeReferenceMock(
                    MockUtils.CreateTypeMock("Class_A", inputAssemblyObj).AddMethod(methodObj).Object)
                .AddMethod(methodObj);

            var algorithm = CreateAlgorithm(new UsedTypesCache());
            var result = algorithm.Process(Enumerable.Repeat(inputTypeRefMock.Object, 1));

            returnTypeMock.Verify(p => p.IsGenericParameter, Times.Once);
            returnTypeMock.Verify(p => p.TypeDefinition, Times.AtLeastOnce);
            returnTypeMock.Verify(p => p.GenericArguments, Times.AtLeastOnce);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.RequiredFor.Count);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    inputAssemblyObj,
                    returnTypeAsmObj
                },
                result.RequiredFor);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }
                
        [Test]
        public void ProcessMethodReferences_ReturnTypeIsDefined_WithParams_ParamsAndReturnTypeInResults()
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly methodParam1TypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            IAssembly methodParam2ATypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_III").Object;
            IAssembly methodParam2BTypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_IV").Object;
            IAssembly methodReturnTypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_V").Object;

            IMethod method1Obj = MockUtils.CreateMethodMock(
                "Method_1",
                returnType: MockUtils.CreateTypeMock("Method_Return", methodReturnTypeAsmObj).ToMemberType(),
                parameters: new[] 
                    { 
                        MockUtils.CreateTypeMock("Method_1_Param_1", methodParam1TypeAsmObj).ToMemberType(),

                        MockUtils.CreateMemberTypeMock(
                            genericArgs: new[]
                                {
                                    MockUtils.CreateTypeMock("Method_1_Param_2_A", methodParam2ATypeAsmObj).ToMemberType(),
                                    MockUtils.CreateTypeMock("Method_1_Param_2_B", methodParam2BTypeAsmObj).ToMemberType()
                                }).Object
                    }).Object;

            Mock<ITypeReference> inputTypeRefMock =
                MockUtils.CreateTypeReferenceMock(
                    MockUtils.CreateTypeMock("Class_A", inputAssemblyObj).AddMethod(method1Obj).Object)
                .AddMethod(method1Obj);

            var algorithm = CreateAlgorithm(new UsedTypesCache());
            var result = algorithm.Process(Enumerable.Repeat(inputTypeRefMock.Object, 1));

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.RequiredFor.Count);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    inputAssemblyObj,
                    methodParam1TypeAsmObj,
                    methodParam2ATypeAsmObj,
                    methodParam2BTypeAsmObj,
                    methodReturnTypeAsmObj
                },
                result.RequiredFor);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        [Test]
        public void ProcessMethodReferences_ReturnTypeIsDefined_WithOverloads_ReturnTypeInResults()
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly method1AParamTypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            IAssembly method1AReturnTypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_III").Object;
            IAssembly method1BParamATypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_IV").Object;
            IAssembly method1BParamBTypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_V").Object;
            IAssembly method1BReturnTypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_VI").Object;

            IMethod method1AObj = MockUtils.CreateMethodMock(
                "Method_1",
                returnType: MockUtils.CreateTypeMock("Method_1A_Return", method1AReturnTypeAsmObj).ToMemberType(),
                parameters: new[] 
                    { 
                        MockUtils.CreateTypeMock("Method_1A_Param", method1AParamTypeAsmObj).ToMemberType() 
                    }).Object;

            IMethod method1BObj = MockUtils.CreateMethodMock(
                "Method_1",
                returnType: MockUtils.CreateTypeMock("Method_1B_Return", method1BReturnTypeAsmObj).ToMemberType(),
                parameters: new[] 
                    { 
                        MockUtils.CreateMemberTypeMock(
                            genericArgs: new[]
                                {
                                    MockUtils.CreateTypeMock("Method_1B_Param_A", method1BParamATypeAsmObj).ToMemberType(),
                                    MockUtils.CreateTypeMock("Method_1B_Param_B", method1BParamBTypeAsmObj).ToMemberType()
                                }
                        ).Object                            
                    }).Object;

            Mock<ITypeReference> inputTypeRefMock =
                MockUtils.CreateTypeReferenceMock(
                    MockUtils.CreateTypeMock("Class_A", inputAssemblyObj)
                    .AddMethod(method1AObj)
                    .AddMethod(method1BObj).Object)
                .AddMethod(method1BObj);

            var algorithm = CreateAlgorithm(new UsedTypesCache());
            var result = algorithm.Process(Enumerable.Repeat(inputTypeRefMock.Object, 1));

            Assert.IsNotNull(result);
            Assert.AreEqual(6, result.RequiredFor.Count);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    inputAssemblyObj,
                    method1AParamTypeAsmObj,
                    method1AReturnTypeAsmObj,
                    method1BParamATypeAsmObj,
                    method1BParamBTypeAsmObj,
                    method1BReturnTypeAsmObj
                },
                result.RequiredFor);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        [Test]
        public void ProcessPropertyReferences_PropertyTypeIsGenericParameter_NoParams_OmitPropertyType()
        {
            VerifyGenericParameterMemberType((tr, mt) => tr.AddProperty("Property_1", mt));
        }

        [Test]
        public void ProcessPropertyReferences_PropertyTypeIsGenericParameter_WithParams_OmitPropertyType()
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly propertyParam1TypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            IAssembly propertyParam2ATypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_III").Object;
            IAssembly propertyParam2BTypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_IV").Object;

            var propertyTypeMock = MockUtils.CreateMemberTypeMock(isGenericParam: true);

            Mock<ITypeReference> inputTypeRefMock =
                MockUtils.CreateTypeReferenceMock(
                    MockUtils.CreateTypeMock("Class_A", inputAssemblyObj).Object)
                .AddProperty(
                    "Property_1",
                    propertyTypeMock.Object,
                    parameters: new[]
                        {
                            MockUtils.CreateTypeMock("Property_1_Param_1", propertyParam1TypeAsmObj).ToMemberType(),

                            MockUtils.CreateMemberTypeMock(
                                genericArgs: new[]
                                    {
                                        MockUtils.CreateTypeMock("Property_1_Param_2_A", propertyParam2ATypeAsmObj).ToMemberType(),
                                        MockUtils.CreateTypeMock("Property_1_Param_2_B", propertyParam2BTypeAsmObj).ToMemberType()
                                    }).Object
                        });

            var algorithm = CreateAlgorithm(new UsedTypesCache());
            var result = algorithm.Process(Enumerable.Repeat(inputTypeRefMock.Object, 1));

            propertyTypeMock.Verify(p => p.IsGenericParameter, Times.Once);
            propertyTypeMock.Verify(p => p.TypeDefinition, Times.Never);
            propertyTypeMock.Verify(p => p.GenericArguments, Times.Never);

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.RequiredFor.Count);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    inputAssemblyObj,
                    propertyParam1TypeAsmObj,
                    propertyParam2ATypeAsmObj,
                    propertyParam2BTypeAsmObj
                },
                result.RequiredFor);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        [Test]
        public void ProcessPropertyReferences_PropertyTypeIsGenericArgument_NoParams_PropertyTypeInResults()
        {
            VerifyGenericArgumentMemberType((tr, mt) => tr.AddProperty("Property_1", mt));
        }

        [Test]
        public void ProcessPropertyReferences_PropertyTypeIsDefined_NoParams_PropertyTypeInResults()
        {
            VerifyTypeDefinitionMemberType((tr, mt) => tr.AddProperty("Property_1", mt));
        }

        [Test]
        public void ProcessPropertyReferences_PropertyTypeIsDefined_WithParams_ParamsAndPropertyTypeInResults()
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly propertyParam1TypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            IAssembly propertyParam2ATypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_III").Object;
            IAssembly propertyParam2BTypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_IV").Object;
            IAssembly propertyTypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_V").Object;

            Mock<ITypeReference> inputTypeRefMock =
                MockUtils.CreateTypeReferenceMock(
                    MockUtils.CreateTypeMock("Class_A", inputAssemblyObj).Object)
                .AddProperty(
                    "Property_1",
                    MockUtils.CreateTypeMock("Property_1_Type", propertyTypeAsmObj).ToMemberType(),
                    parameters: new[] 
                        { 
                            MockUtils.CreateTypeMock("Property_1_Param_1", propertyParam1TypeAsmObj).ToMemberType(),

                            MockUtils.CreateMemberTypeMock(
                                genericArgs: new[]
                                    {
                                        MockUtils.CreateTypeMock("Property_1_Param_2_A", propertyParam2ATypeAsmObj).ToMemberType(),
                                        MockUtils.CreateTypeMock("Property_1_Param_2_B", propertyParam2BTypeAsmObj).ToMemberType()
                                    }).Object
                        });

            var algorithm = CreateAlgorithm(new UsedTypesCache());
            var result = algorithm.Process(Enumerable.Repeat(inputTypeRefMock.Object, 1));

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.RequiredFor.Count);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    inputAssemblyObj,
                    propertyParam1TypeAsmObj,
                    propertyParam2ATypeAsmObj,
                    propertyParam2BTypeAsmObj,
                    propertyTypeAsmObj
                },
                result.RequiredFor);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        [Test]
        public void ProcessEventReferences_EventTypeIsGenericParameter_OmitEventType()
        {
            VerifyGenericParameterMemberType((tr, mt) => tr.AddEvent("Event_1", mt));
        }

        [Test]
        public void ProcessEventReferences_EventTypeIsGenericArgument_EventTypeInResults()
        {
            VerifyGenericArgumentMemberType((tr, mt) => tr.AddEvent("Event_1", mt));
        }

        [Test]
        public void ProcessEventReferences_EventTypeIsDefined_EventTypeInResults()
        {
            VerifyTypeDefinitionMemberType((tr, mt) => tr.AddEvent("Event_1", mt));
        }
        
        [Test]
        public void ProcessFieldReferences_FieldTypeIsGenericParameter_OmitFieldType()
        {
            VerifyGenericParameterMemberType((tr, mt) => tr.AddField("Field_1", mt));
        }

        [Test]
        public void ProcessFieldReferences_FieldTypeIsGenericArgument_FieldTypeInResults()
        {
            VerifyGenericArgumentMemberType((tr, mt) => tr.AddField("Field_1", mt));
        }

        [Test]
        public void ProcessFieldReferences_FieldTypeIsDefined_FieldTypeInResults()
        {
            VerifyTypeDefinitionMemberType((tr, mt) => tr.AddField("Field_1", mt));
        }

        #region Helpers

        private void VerifyGenericParameterMemberType(Action<Mock<ITypeReference>, IMemberType> addMemberTypeFunc)
        {
            var memberTypeMock = MockUtils.CreateMemberTypeMock(isGenericParam: true);

            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            Mock<ITypeReference> inputTypeMock = MockUtils
                .CreateTypeReferenceMock(MockUtils.CreateTypeMock("Class_A", inputAssemblyObj).Object);

            addMemberTypeFunc(inputTypeMock, memberTypeMock.Object);

            var algorithm = CreateAlgorithm(new UsedTypesCache());

            var result = algorithm.Process(Enumerable.Repeat<ITypeReference>(inputTypeMock.Object, 1));

            memberTypeMock.Verify(p => p.IsGenericParameter, Times.Once);
            memberTypeMock.Verify(p => p.TypeDefinition, Times.Never);
            memberTypeMock.Verify(p => p.GenericArguments, Times.Never);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.RequiredFor.Count);
            Assert.That(result.RequiredFor.First(), Is.EqualTo(inputAssemblyObj));
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        private void VerifyGenericArgumentMemberType(Action<Mock<ITypeReference>, IMemberType> addMemberTypeFunc)
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly memberType1AsmObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            IAssembly memberType2AsmObj = MockUtils.CreateAssemblyMock("Assembly_III").Object;

            Mock<IMemberType> memberTypeMock = MockUtils.CreateMemberTypeMock(
                genericArgs: new[]
                {
                    MockUtils.CreateTypeMock("MemberType_Arg_1", memberType1AsmObj).ToMemberType(),
                    MockUtils.CreateTypeMock("MemberType_Arg_2", memberType2AsmObj).ToMemberType()
                });

            Mock<ITypeReference> inputTypeRefMock =
                MockUtils.CreateTypeReferenceMock(
                    MockUtils.CreateTypeMock("Class_A", inputAssemblyObj).Object);

            addMemberTypeFunc(inputTypeRefMock, memberTypeMock.Object);

            var algorithm = CreateAlgorithm(new UsedTypesCache());
            var result = algorithm.Process(Enumerable.Repeat(inputTypeRefMock.Object, 1));

            memberTypeMock.Verify(p => p.IsGenericParameter, Times.Once);
            memberTypeMock.Verify(p => p.TypeDefinition, Times.AtLeastOnce);
            memberTypeMock.Verify(p => p.GenericArguments, Times.AtLeastOnce);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.RequiredFor.Count);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    inputAssemblyObj,
                    memberType1AsmObj,
                    memberType2AsmObj
                },
                result.RequiredFor);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        private void VerifyTypeDefinitionMemberType(Action<Mock<ITypeReference>, IMemberType> addMemberTypeFunc)
        {
            IAssembly inputAssemblyObj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            IAssembly memberTypeAsmObj = MockUtils.CreateAssemblyMock("Assembly_II").Object;

            Mock<IMemberType> memberTypeMock = MockUtils.CreateMemberTypeMock(
                typeDef: MockUtils.CreateTypeMock("MemberType", memberTypeAsmObj).Object);

            Mock<ITypeReference> inputTypeRefMock =
                MockUtils.CreateTypeReferenceMock(MockUtils.CreateTypeMock("Class_A", inputAssemblyObj).Object);

            addMemberTypeFunc(inputTypeRefMock, memberTypeMock.Object);                

            var algorithm = CreateAlgorithm(new UsedTypesCache());
            var result = algorithm.Process(Enumerable.Repeat(inputTypeRefMock.Object, 1));

            memberTypeMock.Verify(p => p.IsGenericParameter, Times.Once);
            memberTypeMock.Verify(p => p.TypeDefinition, Times.AtLeastOnce);
            memberTypeMock.Verify(p => p.GenericArguments, Times.AtLeastOnce);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.RequiredFor.Count);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    inputAssemblyObj,
                    memberTypeAsmObj
                },
                result.RequiredFor);
            Assert.That(result.AlgorithmAdvice, Is.EqualTo(algorithm.GetType().FullName));
        }

        private IAlgorithm<IEnumerable<ITypeReference>> CreateAlgorithm(IUsedTypesCache cache)
        {
            return new ReferencedTypeAlgorithm(
                new ClassHierarchyStrategy(cache),
                new TypeInterfacesStrategy(cache),
                cache);
        }

        #endregion
    }
}
