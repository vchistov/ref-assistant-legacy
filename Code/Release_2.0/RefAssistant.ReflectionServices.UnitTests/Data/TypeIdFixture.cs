using System;
using Lardite.RefAssistant.ReflectionServices.Data;
using NUnit.Framework;
using Moq;

namespace Lardite.RefAssistant.ReflectionServices.UnitTests.Data
{
    [TestFixture]
    public class TypeIdFixture
    {
        const string Int32 = "System.Int32";
        const string Int64 = "System.Int64";

        const string MsCorLib2 = "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        const string MsCorLib4 = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

        [TestCase(null, MsCorLib2)]
        [TestCase("", MsCorLib2)]
        [TestCase(Int32, null)]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void GetId_FullNameIsNull_Exception(string fullName, string asmFullName)
        {
            TypeId.GetId(fullName, GetAssembly(asmFullName));
        }

        [TestCase(Int32, MsCorLib2, null, Int32, MsCorLib2, null, ExpectedResult = true)]
        [TestCase(Int32, MsCorLib2, null, Int64, MsCorLib2, null, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, null, Int32, MsCorLib4, null, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, MsCorLib4, Int32, MsCorLib2, null, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, MsCorLib4, Int32, MsCorLib2, MsCorLib4, ExpectedResult = true)]
        [TestCase(Int32, MsCorLib2, null, null, null, null, ExpectedResult = false)]
        public bool Equals_CompareIds_TrueFalse(string fullName1, string asmFullName1, string frwdFullName1, string fullName2, string asmFullName2, string frwdFullName2)
        {
            var typeId1 = TypeId.GetId(fullName1, GetAssembly(asmFullName1), GetAssembly(frwdFullName1));
            var typeId2 = fullName2 == null ? null : TypeId.GetId(fullName2, GetAssembly(asmFullName2), GetAssembly(frwdFullName2));

            return typeId1.Equals(typeId2);
        }

        [TestCase(Int32, MsCorLib2, null, Int32, MsCorLib2, null, ExpectedResult = true)]
        [TestCase(Int32, MsCorLib2, null, Int64, MsCorLib2, null, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, null, Int32, MsCorLib4, null, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, MsCorLib4, Int32, MsCorLib2, null, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, MsCorLib4, Int32, MsCorLib2, MsCorLib4, ExpectedResult = true)]
        [TestCase(Int32, MsCorLib2, null, null, null, null, ExpectedResult = false)]
        public bool ObjectEquals_CompareIds_TrueFalse(string fullName1, string asmFullName1, string frwdFullName1, string fullName2, string asmFullName2, string frwdFullName2)
        {
            object typeId1 = TypeId.GetId(fullName1, GetAssembly(asmFullName1), GetAssembly(frwdFullName1));
            object typeId2 = fullName2 == null ? null : TypeId.GetId(fullName2, GetAssembly(asmFullName2), GetAssembly(frwdFullName2));

            return typeId1.Equals(typeId2);
        }

        [TestCase(Int32, MsCorLib2, null, Int32, MsCorLib2, null, ExpectedResult = true)]
        [TestCase(Int32, MsCorLib2, null, Int64, MsCorLib2, null, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, null, Int32, MsCorLib4, null, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, MsCorLib4, Int32, MsCorLib2, null, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, MsCorLib4, Int32, MsCorLib2, MsCorLib4, ExpectedResult = true)]
        [TestCase(Int32, MsCorLib2, null, null, null, null, ExpectedResult = false)]
        [TestCase(null, null, null, Int32, MsCorLib2, null, ExpectedResult = false)]
        [TestCase(null, null, null, null, null, null, ExpectedResult = true)]
        public bool OperatorEquals_CompareIds_TrueFalse(string fullName1, string asmFullName1, string frwdFullName1, string fullName2, string asmFullName2, string frwdFullName2)
        {
            var typeId1 = fullName1 == null ? null : TypeId.GetId(fullName1, GetAssembly(asmFullName1), GetAssembly(frwdFullName1));
            var typeId2 = fullName2 == null ? null : TypeId.GetId(fullName2, GetAssembly(asmFullName2), GetAssembly(frwdFullName2));

            return typeId1 == typeId2;
        }

        [TestCase(Int32, MsCorLib2, null, Int32, MsCorLib2, null, ExpectedResult = true)]
        [TestCase(Int32, MsCorLib2, null, Int64, MsCorLib2, null, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, null, Int32, MsCorLib4, null, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, MsCorLib4, Int32, MsCorLib2, null, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, MsCorLib4, Int32, MsCorLib2, MsCorLib4, ExpectedResult = true)]
        [TestCase(Int32, MsCorLib2, null, null, null, null, ExpectedResult = false)]
        [TestCase(null, null, null, Int32, MsCorLib2, null, ExpectedResult = false)]
        [TestCase(null, null, null, null, null, null, ExpectedResult = true)]
        public bool Equals_UseStaticMethodToCompare_TrueFalse(string fullName1, string asmFullName1, string frwdFullName1, string fullName2, string asmFullName2, string frwdFullName2)
        {
            var typeId1 = fullName1 == null ? null : TypeId.GetId(fullName1, GetAssembly(asmFullName1), GetAssembly(frwdFullName1));
            var typeId2 = fullName2 == null ? null : TypeId.GetId(fullName2, GetAssembly(asmFullName2), GetAssembly(frwdFullName2));

            return TypeId.Equals(typeId1, typeId2);
        }

        [TestCase(Int32, MsCorLib2, null, Int32, MsCorLib2, null, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, null, Int64, MsCorLib2, null, ExpectedResult = true)]
        [TestCase(Int32, MsCorLib2, null, Int32, MsCorLib4, null, ExpectedResult = true)]
        [TestCase(Int32, MsCorLib2, MsCorLib4, Int32, MsCorLib2, null, ExpectedResult = true)]
        [TestCase(Int32, MsCorLib2, MsCorLib4, Int32, MsCorLib2, MsCorLib4, ExpectedResult = false)]
        [TestCase(Int32, MsCorLib2, null, null, null, null, ExpectedResult = true)]
        [TestCase(null, null, null, Int32, MsCorLib2, null, ExpectedResult = true)]
        [TestCase(null, null, null, null, null, null, ExpectedResult = false)]
        public bool OperatorNotEquals_CompareIds_TrueFalse(string fullName1, string asmFullName1, string frwdFullName1, string fullName2, string asmFullName2, string frwdFullName2)
        {
            var typeId1 = fullName1 == null ? null : TypeId.GetId(fullName1, GetAssembly(asmFullName1), GetAssembly(frwdFullName1));
            var typeId2 = fullName2 == null ? null : TypeId.GetId(fullName2, GetAssembly(asmFullName2), GetAssembly(frwdFullName2));

            return typeId1 != typeId2;
        }

        #region Helpers

        private static AssemblyId GetAssembly(string fullName)
        {
            return fullName == null ? null : AssemblyId.GetId(fullName);
        }

        #endregion
    }
}
