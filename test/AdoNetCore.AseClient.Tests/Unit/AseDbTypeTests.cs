using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    public class AseDbTypeTests
    {
#if NET_FRAMEWORK
        [TestCaseSource(nameof(Cases))]
        public void AseDbType_Exists_Sap(string aseDbType, int _)
        {
            System.Enum.Parse(typeof(Sybase.Data.AseClient.AseDbType), aseDbType);
        }

        [TestCaseSource(nameof(Cases))]
        public void AseDbType_HasValue_Sap(string aseDbType, int value)
        {
            Assert.AreEqual(value, (int)System.Enum.Parse(typeof(Sybase.Data.AseClient.AseDbType), aseDbType));
        }
#endif

        [TestCaseSource(nameof(Cases))]
        public void AseDbType_Exists(string aseDbType, int _)
        {
            System.Enum.Parse(typeof(AseDbType), aseDbType);
        }

        [TestCaseSource(nameof(Cases))]
        public void AseDbType_HasValue(string aseDbType, int value)
        {
            Assert.AreEqual(value, (int)System.Enum.Parse(typeof(AseDbType), aseDbType));
        }

        public static IEnumerable<TestCaseData> Cases()
        {
            yield return new TestCaseData("BigDateTime", 93);
            yield return new TestCaseData("BigInt", -5);
            yield return new TestCaseData("Binary", -2);
            yield return new TestCaseData("Bit", -7);
            yield return new TestCaseData("Char", 1);
            yield return new TestCaseData("Date", 91);
            yield return new TestCaseData("DateTime", 93);
            yield return new TestCaseData("Decimal", 3);
            yield return new TestCaseData("Double", 8);
            yield return new TestCaseData("Image", -4);
            yield return new TestCaseData("Integer", 4);
            yield return new TestCaseData("Money", -200);
            yield return new TestCaseData("NChar", -204);
            yield return new TestCaseData("Numeric", 2);
            yield return new TestCaseData("NVarChar", -205);
            yield return new TestCaseData("Real", 7);
            yield return new TestCaseData("SmallDateTime", -202);
            yield return new TestCaseData("SmallInt", 5);
            yield return new TestCaseData("SmallMoney", -201);
            yield return new TestCaseData("Text", -1);
            yield return new TestCaseData("Time", 92);
            yield return new TestCaseData("TimeStamp", -203);
            yield return new TestCaseData("TinyInt", -6);
            yield return new TestCaseData("UniChar", -8);
            yield return new TestCaseData("Unitext", -10);
            yield return new TestCaseData("UniVarChar", -9);
            yield return new TestCaseData("UnsignedBigInt", -208);
            yield return new TestCaseData("UnsignedInt", -207);
            yield return new TestCaseData("UnsignedSmallInt", -206);
            yield return new TestCaseData("Unsupported", 0);
            yield return new TestCaseData("VarBinary", -3);
            yield return new TestCaseData("VarChar", 12);
        }
    }
}
