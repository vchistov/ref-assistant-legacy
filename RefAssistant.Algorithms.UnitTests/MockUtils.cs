using System.Collections.Generic;
using System.Linq;
using Lardite.RefAssistant.Algorithms.Contracts;
using Moq;

namespace Lardite.RefAssistant.Algorithms.UnitTests
{
    internal static class MockUtils
    {
        #region Creators
        
        public static Mock<IAssembly> CreateAssemblyMock(
            string name,
            IEnumerable<IAssembly> manifestAssemblies = null)
        {
            var assembly = new Mock<IAssembly>();

            assembly.SetupGet<string>(p => p.Name).Returns(name);
            assembly.Setup(m => m.Equals(It.IsAny<IAssembly>())).Returns<IAssembly>((other) => string.Equals(other.Name, name));
            assembly.SetupGet(m => m.References).Returns(manifestAssemblies);

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
            return CreateTypeMock(fullName, baseType, assembly, forwardedFrom, isInterface, interfaces);
        }

        public static Mock<ITypeDefinition> CreateTypeMock(
            string fullName,
            IAssembly assembly,
            IAssembly importedFrom,
            ITypeDefinition baseType = null,
            IAssembly forwardedFrom = null,
            bool isInterface = false,
            IEnumerable<ITypeDefinition> interfaces = null)
        {
            var type = CreateTypeMock(fullName, baseType, assembly, forwardedFrom, isInterface, interfaces);
            type.SetupGet(m => m.ImportedFrom).Returns(importedFrom);

            return type;
        }

        public static Mock<ITypeReference> CreateTypeReferenceMock(
            ITypeDefinition typeDef)
        {
            var type = new Mock<ITypeReference>();
            type.SetupGet(p => p.TypeDefinition).Returns(typeDef);

            return type;
        }

        public static Mock<IMemberType> CreateMemberTypeMock(
            ITypeDefinition typeDef = null,
            IEnumerable<IMemberType> genericArgs = null,
            bool isGenericParam = false)
        {
            var memberType = new Mock<IMemberType>();

            memberType.SetupGet(p => p.IsGenericParameter).Returns(isGenericParam);
            memberType.SetupGet(p => p.TypeDefinition).Returns(typeDef);
            memberType.SetupGet(p => p.GenericArguments).Returns(genericArgs == null ? genericArgs : genericArgs.ToArray());

            return memberType;
        }

        public static Mock<IMethod> CreateMethodMock(
            string methodName,
            IMemberType returnType = null,
            IEnumerable<IMemberType> parameters = null)
        {
            var method = new Mock<IMethod>();

            method.SetupGet(p => p.Name).Returns(methodName);
            method.SetupGet(p => p.ReturnType).Returns(returnType);
            method.SetupGet(p => p.Parameters).Returns(parameters == null ? null : parameters.Select(p => CreateMemberParameter(p)).ToArray());

            return method;
        }

        public static Mock<IProperty> CreatePropertyMock(
            string propertyName,
            IMemberType propertyType,
            IEnumerable<IMemberType> parameters = null)
        {
            var property = new Mock<IProperty>();

            property.SetupGet(p => p.Name).Returns(propertyName);
            property.SetupGet(p => p.PropertyType).Returns(propertyType);
            property.SetupGet(p => p.Parameters).Returns(parameters == null ? null : parameters.Select(p => CreateMemberParameter(p)).ToArray());

            return property;
        }

        public static Mock<IEvent> CreateEventMock(
            string eventName,
            IMemberType eventType)
        {
            var @event = new Mock<IEvent>();

            @event.SetupGet(p => p.Name).Returns(eventName);
            @event.SetupGet(p => p.EventType).Returns(eventType);

            return @event;
        }

        public static Mock<IField> CreateFieldMock(
            string fieldName,
            IMemberType fieldType)
        {
            var field = new Mock<IField>();

            field.SetupGet(p => p.Name).Returns(fieldName);
            field.SetupGet(p => p.FieldType).Returns(fieldType);

            return field;
        }

        public static ITypeDefinition CreateSystemVoidType()
        {
            var assembly = MockUtils.CreateAssemblyMock("mscorlib").Object;

            return MockUtils.CreateTypeMock(
                "System.Void",
                assembly,
                baseType: MockUtils.CreateTypeMock(
                    "System.ValueType",
                    assembly,
                    baseType: MockUtils.CreateTypeMock(
                        "System.Object",
                        assembly).Object).Object).Object;
        }

        #endregion

        #region Extension methods

        public static Mock<ITypeDefinition> AddMethod(
            this Mock<ITypeDefinition> @this,
            string methodName,
            IMemberType returnType = null,
            IEnumerable<IMemberType> parameters = null)
        {
            var method = MockUtils.CreateMethodMock(
                methodName,
                returnType ?? MockUtils.CreateSystemVoidType().ToMemberType(),
                parameters).Object;

            return @this.AddMethod(method);
        }

        public static Mock<ITypeDefinition> AddMethod(
            this Mock<ITypeDefinition> @this,
            IMethod method)
        {
            return @this.SetupUnionWith(p => p.Methods, method);
        }

        public static Mock<ITypeReference> AddMethod(
            this Mock<ITypeReference> @this,
            string methodName,
            IMemberType returnType = null,
            IEnumerable<IMemberType> parameters = null)
        {
            var method = MockUtils.CreateMethodMock(
                methodName,
                returnType ?? MockUtils.CreateSystemVoidType().ToMemberType(),
                parameters).Object;

            return @this.AddMethod(method);
        }

        public static Mock<ITypeReference> AddMethod(
            this Mock<ITypeReference> @this,
            IMethod method)
        {
            return @this.SetupUnionWith(p => p.MethodReferences, method);
        }

        public static Mock<ITypeDefinition> AddProperty(
            this Mock<ITypeDefinition> @this,
            string propertyName,
            IMemberType propertyType,
            IEnumerable<IMemberType> parameters = null)
        {
            var property = MockUtils.CreatePropertyMock(
                propertyName,
                propertyType ?? MockUtils.CreateSystemVoidType().ToMemberType(),
                parameters).Object;

            return @this.SetupUnionWith(p => p.Properties, property);
        }

        public static Mock<ITypeReference> AddProperty(
            this Mock<ITypeReference> @this,
            string propertyName,
            IMemberType propertyType,
            IEnumerable<IMemberType> parameters = null)
        {
            var property = MockUtils.CreatePropertyMock(
                propertyName,
                propertyType ?? MockUtils.CreateSystemVoidType().ToMemberType(),
                parameters).Object;

            return @this.SetupUnionWith(p => p.PropertyReferences, property);
        }

        public static Mock<ITypeDefinition> AddEvent(
            this Mock<ITypeDefinition> @this,
            string eventName,
            IMemberType eventType)
        {
            var @event = MockUtils.CreateEventMock(
                eventName,
                eventType).Object;

            return @this.SetupUnionWith(p => p.Events, @event);
        }

        public static Mock<ITypeReference> AddEvent(
            this Mock<ITypeReference> @this,
            string eventName,
            IMemberType eventType)
        {
            var @event = MockUtils.CreateEventMock(
                eventName,
                eventType).Object;

            return @this.SetupUnionWith(p => p.EventReferences, @event);
        }

        public static Mock<ITypeDefinition> AddField(
            this Mock<ITypeDefinition> @this,
            string fieldName,
            IMemberType fieldType)
        {
            var field = MockUtils.CreateFieldMock(
                fieldName,
                fieldType).Object;

            return @this.SetupUnionWith(p => p.Fields, field);
        }

        public static Mock<ITypeReference> AddField(
            this Mock<ITypeReference> @this,
            string fieldName,
            IMemberType fieldType)
        {
            var field = MockUtils.CreateFieldMock(
                fieldName,
                fieldType).Object;

            return @this.SetupUnionWith(p => p.FieldReferences, field);
        }

        public static IMemberType ToMemberType(this ITypeDefinition @this)
        {
            return MockUtils.CreateMemberTypeMock(@this).Object;
        }

        public static IMemberType ToMemberType(this Mock<ITypeDefinition> @this)
        {
            return ToMemberType(@this.Object);
        }

        #endregion

        #region Helpers

        private static Mock<ITypeDefinition> CreateTypeMock(
            string fullName,
            ITypeDefinition baseType,
            IAssembly assembly,            
            IAssembly forwardedFrom,
            bool isInterface,
            IEnumerable<ITypeDefinition> interfaces)
        {
            var type = new Mock<ITypeDefinition>();

            type.SetupGet<TypeName>(p => p.Name).Returns(new TypeName(It.IsAny<ITypeDefinition>()) { FullName = fullName });
            type.Setup(m => m.Equals(It.IsAny<ITypeDefinition>())).Returns<ITypeDefinition>((other) => string.Equals(other.Name.FullName, fullName));
            type.SetupGet(m => m.Assembly).Returns(assembly);
            type.SetupGet(m => m.BaseType).Returns(baseType);
            type.SetupGet(m => m.ForwardedFrom).Returns(forwardedFrom);
            type.SetupGet(m => m.IsInterface).Returns(isInterface);
            type.SetupGet(m => m.Interfaces).Returns(interfaces ?? Enumerable.Empty<ITypeDefinition>());

            return type;
        }

        private static IMemberParameter CreateMemberParameter(IMemberType memberType)
        {
            var memberParam = new Mock<IMemberParameter>();
            memberParam.SetupGet(p => p.ParameterType).Returns(memberType);
            return memberParam.Object;
        }

        #endregion
    }
}
