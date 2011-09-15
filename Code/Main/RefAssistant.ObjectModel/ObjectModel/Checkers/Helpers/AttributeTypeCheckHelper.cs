//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com),
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.Linq;

using Lardite.RefAssistant.Extensions;

using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel.Checkers.Helpers
{
    /// <summary>
    /// Helper for checking type's attributes.
    /// </summary>
    sealed class AttributeTypeCheckHelper
    {
        #region Fields

        private InterfaceCheckHelper _interfaceCheckHelper;
        private CheckerSharedData _sharedData;
        private List<Action<TypeDefinition>> _checkActionList;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of a <see cref="AttributeTypeCheckHelper"/> class.
        /// </summary>
        public AttributeTypeCheckHelper()
        {
            _interfaceCheckHelper = new InterfaceCheckHelper();
            _checkActionList = new List<Action<TypeDefinition>>
                {
                    CheckType,
                    CheckMethods,
                    CheckProperties,
                    CheckFields,
                    CheckEvents
                };
        }

        #endregion // .ctor

        #region Public methods

        /// <summary>
        /// Performs type's attributes checking.
        /// </summary>
        /// <param name="type">The specified type.</param>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        public void Check(TypeDefinition type, CheckerSharedData sharedData)
        {
            try
            {
                _sharedData = sharedData;

                foreach (var checkAction in _checkActionList)
                {
                    checkAction(type);
                    if (!_sharedData.HasCandidateReferences)
                        return;
                }
            }
            catch (Exception ex)
            {
                throw Error.CheckType(type.AssemblyQualifiedName(),
                    string.Format(Resources.CleanExecutor_CheckTypeAttributesException, type.AssemblyQualifiedName()), ex);
            }
        }

        #endregion // Public methods

        #region Private methods

        #region Check type attributes

        /// <summary>
        /// Check type's attributes.
        /// </summary>
        /// <param name="typeDef">The type definition which will be investigated.</param>
        private void CheckType(TypeDefinition typeDef)
        {
            if (typeDef.HasCustomAttributes)
            {
                var customAttributeArgs = typeDef.CustomAttributes.SelectMany(ca => GetCustomAttributeArgument(ca));
                CheckCustomAttributes(customAttributeArgs);
            }
        }

        #endregion // Check type attributes

        #region Check methods attributes

        /// <summary>
        /// Check methods' attributes.
        /// </summary>
        /// <param name="type">The type definition which will be investigated.</param>
        private void CheckMethods(TypeDefinition typeDef)
        {
            var methods = typeDef.Methods.Where(m => IsMethodContainsAttribute(m));
            foreach (var method in methods)
            {
                CheckMethodCustomAttributes(method);
                if (!_sharedData.HasCandidateReferences)
                    return;

                CheckMethodReturnAttributes(method.MethodReturnType);
                if (!_sharedData.HasCandidateReferences)
                    return;
            }
        }

        /// <summary>
        /// Check whether method has custom attributes.
        /// </summary>
        /// <param name="method">The method definition.</param>
        /// <returns>Returns true if method has custom attributes; otherwise false.</returns>
        private bool IsMethodContainsAttribute(MethodDefinition method)
        {
            var methodInfo = method.GetElementMethod();

            return method.HasCustomAttributes ||
                methodInfo.MethodReturnType.HasCustomAttributes ||
                methodInfo.MethodReturnType.HasFieldMarshal ||
                methodInfo.MethodReturnType.HasMarshalInfo;
        }

        /// <summary>
        /// Check arguments of custom attribute of a method.
        /// </summary>
        /// <param name="methodDef">The method definition.</param>
        private void CheckMethodCustomAttributes(MethodDefinition methodDef)
        {
            if (methodDef.HasCustomAttributes)
            {
                var customAttributeArgs = methodDef.CustomAttributes.SelectMany(ca => GetCustomAttributeArgument(ca));
                CheckCustomAttributes(customAttributeArgs);
            }
        }

        /// <summary>
        /// Check arguments of custom attributes of a method for the return type.
        /// </summary>
        /// <param name="returnType">The method return type.</param>
        private void CheckMethodReturnAttributes(MethodReturnType returnType)
        {
            if (returnType.HasCustomAttributes)
            {
                var customAttributeArgs = returnType.CustomAttributes.SelectMany(ca => GetCustomAttributeArgument(ca));
                CheckCustomAttributes(customAttributeArgs);
            }

            if (returnType.HasMarshalInfo)
            {
                CheckMarshalInfo(returnType.MarshalInfo);
            }
        }



        #endregion // Check methods attributes

        #region Check properties attributes

        /// <summary>
        /// Check properties' attributes.
        /// </summary>
        /// <param name="typeDef">The type definition which will be investigated.</param>
        private void CheckProperties(TypeDefinition typeDef)
        {
            var properties = typeDef.Properties.Where(p => p.HasCustomAttributes);
            foreach (var property in properties)
            {
                var customAttributeArgs = property.CustomAttributes.SelectMany(ca => GetCustomAttributeArgument(ca));
                CheckCustomAttributes(customAttributeArgs);

                if (!_sharedData.HasCandidateReferences)
                    return;
            }
        }

        #endregion // Check properties attributes

        #region Check fields attributes

        /// <summary>
        /// Check fields' attributes.
        /// </summary>
        /// <param name="typeDef">The type definition which will be investigated.</param>
        private void CheckFields(TypeDefinition typeDef)
        {
            var fields = typeDef.Fields.Where(f => f.HasCustomAttributes || f.HasMarshalInfo);
            foreach (var field in fields)
            {
                CheckFieldCustomAttributes(field);
                if (!_sharedData.HasCandidateReferences)
                    return;

                CheckMarshalInfo(field.MarshalInfo);
                if (!_sharedData.HasCandidateReferences)
                    return;
            }
        }

        /// <summary>
        /// Check arguments of custom attribute of a field.
        /// </summary>
        /// <param name="fieldDef">The method definition.</param>
        private void CheckFieldCustomAttributes(FieldDefinition fieldDef)
        {
            if (fieldDef.HasCustomAttributes)
            {
                var customAttributeArgs = fieldDef.CustomAttributes.SelectMany(ca => GetCustomAttributeArgument(ca));
                CheckCustomAttributes(customAttributeArgs);
            }
        }

        #endregion // Check fields attributes

        #region Check events attributes

        /// <summary>
        /// Check events' attributes.
        /// </summary>
        /// <param name="typeDef">The type definition which will be investigated.</param>
        private void CheckEvents(TypeDefinition typeDef)
        {
            var events = typeDef.Events.Where(e => e.HasCustomAttributes);
            foreach (var e in events)
            {
                var customAttributeArgs = e.CustomAttributes.SelectMany(ca => GetCustomAttributeArgument(ca));
                CheckCustomAttributes(customAttributeArgs);
            }
        }

        #endregion // Check events attributes

        #region Common logic

        /// <summary>
        /// Check arguments of custom attribute.
        /// </summary>
        /// <param name="attributeArgs">The list of custom attribute arguments (types).</param>
        private void CheckCustomAttributes(IEnumerable<CustomAttributeArgument> attributeArgs)
        {
            IMetadataScope forwardedFrom;
            foreach (CustomAttributeArgument customAttributeArgument in attributeArgs)
            {
                TypeDefinition typeDefinition = customAttributeArgument.Value is TypeReference
                    ? (customAttributeArgument.Value as TypeReference).Resolve(out forwardedFrom)
                    : customAttributeArgument.Type.Resolve(out forwardedFrom);

                _sharedData.RemoveFromCandidates(forwardedFrom);

                if (typeDefinition != null && !_sharedData.IsUsedTypeExists(typeDefinition.AssemblyQualifiedName()))
                {
                    _sharedData.RemoveFromCandidates(typeDefinition.Scope);
                    if (!_sharedData.HasCandidateReferences)
                        return;

                    _sharedData.AddToUsedTypes(typeDefinition.AssemblyQualifiedName());
                    _interfaceCheckHelper.Check(typeDefinition, _sharedData);
                }
            }
        }

        /// <summary>
        /// Collect all arguments of attribute (.ctor args, parameters types, field types).
        /// </summary>
        /// <param name="customAttribute">The custom attribute instance.</param>
        /// <returns>Returns collection of attibutes' arguments.</returns>
        private IEnumerable<CustomAttributeArgument> GetCustomAttributeArgument(CustomAttribute customAttribute)
        {
            var result = new List<CustomAttributeArgument>();

            if (customAttribute.HasConstructorArguments)
            {
                result.AddRange(customAttribute.ConstructorArguments);
            }

            if (customAttribute.HasProperties)
            {
                result.AddRange(customAttribute.Properties.Select(p => p.Argument));
            }

            if (customAttribute.HasFields)
            {
                result.AddRange(customAttribute.Fields.Select(f => f.Argument));
            }

            return result;
        }

        /// <summary>
        /// Check marshal info. Nees to resolve type reference if marshal is CustomMarshaller
        /// </summary>
        /// <param name="marshalInfo">The marshal inforamtion.</param>
        private void CheckMarshalInfo(MarshalInfo marshalInfo)
        {
            var cmi = marshalInfo as CustomMarshalInfo;
            if (cmi != null && cmi.ManagedType != null)
            {
                IMetadataScope forwardedFrom;
                var typeDef = cmi.ManagedType.Resolve(out forwardedFrom);
                _sharedData.RemoveFromCandidates(forwardedFrom);

                if (typeDef != null)
                {
                    _sharedData.RemoveFromCandidates(typeDef.Scope);
                    _sharedData.AddToUsedTypes(typeDef.AssemblyQualifiedName());
                }
            }
        }

        #endregion // Common logic

        #endregion // Private methods
    }
}
