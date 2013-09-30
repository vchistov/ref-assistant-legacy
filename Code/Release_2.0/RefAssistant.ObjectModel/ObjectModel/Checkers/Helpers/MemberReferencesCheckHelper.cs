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
    /// Helper for checking referenced members of assemblies.
    /// </summary>
    sealed class MemberReferencesCheckHelper
    {
        #region Fields

        private MemberParametersCheckHelper _helper = new MemberParametersCheckHelper();

        #endregion // Fields

        #region Public methods

        /// <summary>
        /// Performs checking of referenced members of the specified type.
        /// </summary>
        /// <param name="type">The specified type.</param>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        /// <param name="evaluator">The project evaluator.</param>
        public void Check(TypeReference typeRef, CheckerSharedData sharedData, IProjectEvaluator evaluator)
        {
            try
            {
                foreach (MemberReference memberRef in GetMemberReferences(typeRef, evaluator))
                {
                    EvaluateMemberReference(memberRef, sharedData);

                    if (!sharedData.HasCandidateReferences)
                        return;
                }
            }
            catch (Exception ex)
            {
                throw Error.CheckType(typeRef.AssemblyQualifiedName(),
                    string.Format(Resources.MemberReferencesCheckHelper_CheckTypeException, typeRef.AssemblyQualifiedName()), ex);
            }
        }

        #endregion // Public methods

        #region Private methods

        /// <summary>
        /// Analyse members reference (is it method? it it overloaded? etc).
        /// </summary>
        /// <param name="memberRef">The analysed member reference.</param>        
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        private void EvaluateMemberReference(MemberReference memberRef, CheckerSharedData sharedData)
        {
            if (!(memberRef is MethodReference) ||
                (!(memberRef as MethodReference).HasParameters))
            {
                CheckMemberReference(memberRef, sharedData);
                return;
            }

            var methodRef = memberRef as MethodReference;
            IEnumerable<MethodDefinition> methods = GetOverloadedMethods(memberRef.DeclaringType, methodRef.Name, methodRef.Parameters.Count);

            if (methods.Count() > 1)
            {
                CheckOverloadedMethods(methods, sharedData);
            }
            else
            {
                CheckMemberReference(memberRef, sharedData);
            }
        }

        /// <summary>
        /// Check not-overloaded member reference.
        /// </summary>
        /// <param name="memberRef">The member reference.</param>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        private void CheckMemberReference(MemberReference memberRef, CheckerSharedData sharedData)
        {
            try
            {
                _helper.Check(memberRef, sharedData);
            }
            catch (Exception ex)
            {
                throw Error.CheckType(memberRef.DeclaringType.AssemblyQualifiedName(),
                    string.Format(Resources.MemberReferencesCheckHelper_CheckMemberReferenceException, 
                        memberRef.FullName, memberRef.Module.AssemblyQualifiedName()), ex);
            }
        }

        /// <summary>
        /// Check overloaded methods.
        /// </summary>
        /// <param name="methods">The overloaded methods.</param>
        /// <param name="sharedData">Used types and assemblies candidates.</param>
        private void CheckOverloadedMethods(IEnumerable<MethodDefinition> methods, CheckerSharedData sharedData)
        {
            try
            {
                foreach (var method in methods)
                {
                    _helper.CheckMethodParameters(method, sharedData);
                }
            }
            catch (Exception ex)
            {
                var method = methods.First();
                string methodName = string.Format("{0}.{1}", method.DeclaringType.FullName, method.Name);
                int parametersAmount = method.Parameters.Count;

                throw Error.CheckType(method.DeclaringType.AssemblyQualifiedName(),
                    string.Format(Resources.MemberReferencesCheckHelper_CheckOverloadedMethodsException,
                        methodName, parametersAmount, method.Module.AssemblyQualifiedName()), ex);
            }
        }

        /// <summary>
        /// Get list of referenced members of the specified type.
        /// </summary>
        /// <param name="typeRef">The type reference.</param>
        /// <param name="evaluator">The project evaluator.</param>
        /// <returns>Returns list of members references.</returns>
        private IEnumerable<MemberReference> GetMemberReferences(TypeReference typeRef, IProjectEvaluator evaluator)
        {
            var typeName = typeRef.FullName;
            return evaluator.MemberReferences
                .Where(item => item.DeclaringType.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get overloaded methods of specified type.
        /// </summary>
        /// <param name="typeRef">The type reference.</param>
        /// <param name="methodName">The name of overloaded method.</param>
        /// <param name="methodParamsAmount">The parameters amount of overloaded methods.</param>
        /// <returns>Returns list of overloaded methods having specified name and parameters amount.</returns>
        private IEnumerable<MethodDefinition> GetOverloadedMethods(TypeReference typeRef, string methodName, int methodParamsAmount)
        {
            // we won't pay attention that type was forwarded, more important for us it's to resolve type.
            IMetadataScope forwardedFrom;
            return from method in typeRef.Resolve(out forwardedFrom).Methods
                   where method.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)
                     && method.Parameters.Count == methodParamsAmount
                   select method;
        }
        
        #endregion // Private methods
    }
}
