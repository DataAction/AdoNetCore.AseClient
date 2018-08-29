using System;
using System.Collections.Generic;
using System.Data;
using AdoNetCore.AseClient.Tests.ParameterProvider;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
#if NET_FRAMEWORK
    [TestFixture(typeof(SapParameterProvider))]
#endif
    [TestFixture(typeof(CoreFxParameterProvider))]
    public class AseParameterComparisonTests<T> where T : IParameterProvider
    {
        private T GetParameterProvider()
        {
            return Activator.CreateInstance<T>();
        }

        [TestCaseSource(nameof(CreateAseParameter_WithAseDbType_HasExpectedDbType_Cases))]
        public void CreateAseParameter_WithAseDbType_HasExpectedDbType(string aseDbType, DbType expectedType)
        {
            var p = GetParameterProvider().GetParameter("@test", aseDbType);
            Assert.AreEqual(expectedType, p.DbType);
        }

        public static IEnumerable<TestCaseData> CreateAseParameter_WithAseDbType_HasExpectedDbType_Cases()
        {
            yield return new TestCaseData("BigDateTime", DbType.DateTime);
            yield return new TestCaseData("BigInt", DbType.Int64);
            yield return new TestCaseData("Binary", DbType.Binary);
            yield return new TestCaseData("Bit", DbType.Boolean);
            yield return new TestCaseData("Char", DbType.AnsiStringFixedLength);
            yield return new TestCaseData("Date", DbType.Date);
            yield return new TestCaseData("DateTime", DbType.DateTime);
            yield return new TestCaseData("Decimal", DbType.Decimal);
            yield return new TestCaseData("Double", DbType.Double);
            yield return new TestCaseData("Image", DbType.Binary);
            yield return new TestCaseData("Integer", DbType.Int32);
            yield return new TestCaseData("Money", DbType.Currency);
            yield return new TestCaseData("NChar", DbType.AnsiStringFixedLength);
            yield return new TestCaseData("Numeric", DbType.VarNumeric);
            yield return new TestCaseData("NVarChar", DbType.AnsiString);
            yield return new TestCaseData("Real", DbType.Single);
            yield return new TestCaseData("SmallDateTime", DbType.DateTime);
            yield return new TestCaseData("SmallInt", DbType.Int16);
            yield return new TestCaseData("SmallMoney", DbType.Currency);
            yield return new TestCaseData("Text", DbType.AnsiString);
            yield return new TestCaseData("Time", DbType.Time);
            yield return new TestCaseData("TimeStamp", DbType.Binary);
            yield return new TestCaseData("TinyInt", DbType.Byte);
            yield return new TestCaseData("UniChar", DbType.StringFixedLength);
            yield return new TestCaseData("Unitext", DbType.String);
            yield return new TestCaseData("UniVarChar", DbType.String);
            yield return new TestCaseData("UnsignedBigInt", DbType.UInt64);
            yield return new TestCaseData("UnsignedInt", DbType.UInt32);
            yield return new TestCaseData("UnsignedSmallInt", DbType.UInt16);
            yield return new TestCaseData("Unsupported", DbType.Guid);
            yield return new TestCaseData("VarBinary", DbType.Binary);
            yield return new TestCaseData("VarChar", DbType.AnsiString);
        }

        [TestCaseSource(nameof(CreateAseParameter_WithValue_Cases))]
        public void CreateAseParameter_WithValue(object value)
        {
            var p = GetParameterProvider().GetParameter("@test", value);

            Assert.AreEqual(DbType.Guid, p.DbType);
            Assert.AreEqual(0, p.Size);
            Assert.AreEqual(ParameterDirection.Input, p.Direction);
            Assert.AreEqual(false, p.IsNullable);
            Assert.AreEqual(0, p.Precision);
            Assert.AreEqual(0, p.Scale);
            Assert.IsNull(p.SourceColumn);
            Assert.AreEqual(value, p.Value);

#if NET_FRAMEWORK
            if (p is Sybase.Data.AseClient.AseParameter pSap)
            {
                Assert.AreEqual("Unsupported", pSap.AseDbType.ToString());
            }
#endif
            if (p is AseParameter pCore)
            {
                Assert.AreEqual("Unsupported", pCore.AseDbType.ToString());
            }
        }

        public static IEnumerable<TestCaseData> CreateAseParameter_WithValue_Cases()
        {
            yield return new TestCaseData('a');
            yield return new TestCaseData(string.Empty);
            yield return new TestCaseData("a");
            yield return new TestCaseData(DateTime.Now);
            yield return new TestCaseData(1);
            yield return new TestCaseData(1m);
            yield return new TestCaseData(1L);
            yield return new TestCaseData((short)1);
            yield return new TestCaseData((byte)1);
            yield return new TestCaseData(true);
            yield return new TestCaseData(Guid.NewGuid());
            yield return new TestCaseData(new byte[0]);
            yield return new TestCaseData(new byte[]{ 1 });
        }
    }
}
