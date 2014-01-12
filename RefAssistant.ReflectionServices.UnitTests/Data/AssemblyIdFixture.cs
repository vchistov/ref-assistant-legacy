using System;
using Lardite.RefAssistant.ReflectionServices.Data;
using NUnit.Framework;

namespace Lardite.RefAssistant.ReflectionServices.UnitTests.Data
{
    [TestFixture]
    public class AssemblyIdFixture
    {
        const string MsCorLib2 = "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        const string MsCorLib4 = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void GetId_FullNameIsNull_Exception(string fullName)
        {
            AssemblyId.GetId(fullName);
        }

        [TestCase(MsCorLib4, MsCorLib4, ExpectedResult = true)]
        [TestCase(MsCorLib2, MsCorLib4, ExpectedResult = false)]
        [TestCase(MsCorLib2, null, ExpectedResult = false)]
        public bool Equals_CompareIds_TrueFalse(string fullName1, string fullName2)
        {
            var assemblyId1 = AssemblyId.GetId(fullName1);
            var assemblyId2 = fullName2 == null ? null : AssemblyId.GetId(fullName2);

            return assemblyId1.Equals(assemblyId2);
        }

        [TestCase(MsCorLib4, MsCorLib4, ExpectedResult = true)]
        [TestCase(MsCorLib2, MsCorLib4, ExpectedResult = false)]
        [TestCase(MsCorLib2, null, ExpectedResult = false)]
        [TestCase(null, MsCorLib4, ExpectedResult = false)]
        [TestCase(null, null, ExpectedResult = true)]
        public bool OperatorEquals_CompareIds_TrueFalse(string fullName1, string fullName2)
        {
            var assemblyId1 = fullName1 == null ? null : AssemblyId.GetId(fullName1);
            var assemblyId2 = fullName2 == null ? null : AssemblyId.GetId(fullName2);

            return assemblyId1 == assemblyId2;
        }

        [TestCase(MsCorLib4, MsCorLib4, ExpectedResult = false)]
        [TestCase(MsCorLib2, MsCorLib4, ExpectedResult = true)]
        [TestCase(MsCorLib2, null, ExpectedResult = true)]
        [TestCase(null, MsCorLib4, ExpectedResult = true)]
        [TestCase(null, null, ExpectedResult = false)]
        public bool OperatorNotEquals_CompareIds_TrueFalse(string fullName1, string fullName2)
        {
            var assemblyId1 = fullName1 == null ? null : AssemblyId.GetId(fullName1);
            var assemblyId2 = fullName2 == null ? null : AssemblyId.GetId(fullName2);

            return assemblyId1 != assemblyId2;
        }

        [TestCase(null, null, ExpectedResult = true)]
        [TestCase(MsCorLib4, null, ExpectedResult = false)]
        [TestCase(null, MsCorLib4, ExpectedResult = false)]
        [TestCase(MsCorLib4, MsCorLib4, ExpectedResult = true)]
        [TestCase(MsCorLib2, MsCorLib4, ExpectedResult = false)]
        public bool Equals_UseStaticMethodToCompare_TrueFalse(string fullName1, string fullName2)
        {
            AssemblyId assemblyId1 = fullName1 == null ? null : AssemblyId.GetId(fullName1);
            AssemblyId assemblyId2 = fullName2 == null ? null : AssemblyId.GetId(fullName2);

            return AssemblyId.Equals(assemblyId1, assemblyId2);
        }
    }
}
