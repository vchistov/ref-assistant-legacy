//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using Lardite.RefAssistant.Extensions;

using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel.Checkers.Helpers
{
    /// <summary>
    /// Helper for checking parameters of member reference.
    /// </summary>
    sealed class MemberParametersCheckHelper
    {
        #region Fields

        private readonly ClassCheckHelper _classCheckHeper;
        private readonly InterfaceCheckHelper _interfaceCheckHelper;

        #endregion // Fields

        #region .ctor

        public MemberParametersCheckHelper()
        {
            _classCheckHeper = new ClassCheckHelper();
            _interfaceCheckHelper = new InterfaceCheckHelper();
        }

        #endregion // .ctor

        #region Public methods

        /// <summary>
        /// Performs checking of member reference types.
        /// </summary>
        /// <param name="memberRef">The specified member reference.</param>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        public void Check(MemberReference memberRef, CheckerSharedData sharedData)
        {
            if (memberRef is MethodReference)
            {
                CheckMethodParameters(memberRef as MethodReference, sharedData);
            }
            else if (memberRef is PropertyReference)
            {
                CheckPropertyParameters(memberRef as PropertyReference, sharedData);
            }
            else if (memberRef is FieldReference)
            {
                CheckFieldType(memberRef as FieldReference, sharedData);
            }
            else if (memberRef is EventReference)
            {
                CheckEventType(memberRef as EventReference, sharedData);
            }
        }

        /// <summary>
        /// Performs checking method's parameters.
        /// </summary>
        /// <param name="memberRef">The specified method.</param>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        internal void CheckMethodParameters(MethodReference methodRef, CheckerSharedData sharedData)
        {
            ResolveTypeReference(methodRef.ReturnType, sharedData);
            if (methodRef.HasParameters)
            {
                foreach (var paramDef in methodRef.Parameters)
                {
                    ResolveTypeReference(paramDef.ParameterType, sharedData);
                }
            }
        }

        /// <summary>
        /// Performs checking property's parameters.
        /// </summary>
        /// <param name="memberRef">The specified property.</param>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        internal void CheckPropertyParameters(PropertyReference propertyRef, CheckerSharedData sharedData)
        {
            ResolveTypeReference(propertyRef.PropertyType, sharedData);
            foreach (var paramDef in propertyRef.Parameters)
            {
                ResolveTypeReference(paramDef.ParameterType, sharedData);
            }
        }

        /// <summary>
        /// Performs checking field's type.
        /// </summary>
        /// <param name="memberRef">The specified field.</param>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        internal void CheckFieldType(FieldReference fieldRef, CheckerSharedData sharedData)
        {
            ResolveTypeReference(fieldRef.FieldType, sharedData);
        }

        /// <summary>
        /// Performs checking event's type.
        /// </summary>
        /// <param name="memberRef">The specified event.</param>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        internal void CheckEventType(EventReference eventRef, CheckerSharedData sharedData)
        {
            ResolveTypeReference(eventRef.EventType, sharedData);
        }

        #endregion // Public methods

        #region Private methods

        private void ResolveTypeReference(TypeReference typeRefParam, CheckerSharedData sharedData)
        {
            if (typeRefParam == null || typeRefParam.IsGenericParameter
                || (typeRefParam.IsArray && ((ArrayType)typeRefParam).ElementType.IsGenericParameter))                
            {
                return;
            }

            var typeRef = (typeRefParam.IsArray) 
                ? ((ArrayType)typeRefParam).ElementType 
                : typeRefParam; 

            IMetadataScope forwardedFrom;
            var typeDef = typeRef.Resolve(out forwardedFrom);
            sharedData.RemoveFromCandidates(forwardedFrom);
            if (typeDef != null && !sharedData.IsUsedTypeExists(typeDef.AssemblyQualifiedName()))
            {
                sharedData.RemoveFromCandidates(typeDef.Scope);

                _classCheckHeper.Check(typeDef, sharedData);
                _interfaceCheckHelper.Check(typeDef, sharedData);
            }
            else if (typeDef == null)
            {
                sharedData.AddToUsedTypes(typeRef.AssemblyQualifiedName());
                sharedData.RemoveFromCandidates(typeRef.Scope);
            }
        }

        #endregion // Private methods
    }
}
