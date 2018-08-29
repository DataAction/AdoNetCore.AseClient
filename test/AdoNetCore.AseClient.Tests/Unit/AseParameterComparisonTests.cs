using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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

        private void AssertParameterTypes(DbParameter p, DbType expectedDbType, string expectedAseDbType)
        {
            Assert.AreEqual(expectedDbType, p.DbType);

#if NET_FRAMEWORK
            if (p is Sybase.Data.AseClient.AseParameter pSap)
            {
                Assert.AreEqual(expectedAseDbType, pSap.AseDbType.ToString());
            }
#endif
            if (p is AseParameter pCore)
            {
                Assert.AreEqual(expectedAseDbType, pCore.AseDbType.ToString());
            }
        }

        [TestCaseSource(nameof(CreateAseParameter_WithAseDbType_HasExpectedDbType_Cases))]
        public void CreateAseParameter_WithAseDbType_HasExpectedDbType(string aseDbType, DbType expectedDbType, string expectedAseDbType)
        {
            var p = GetParameterProvider().GetParameter("@test", aseDbType);

            AssertParameterTypes(p, expectedDbType, expectedAseDbType);
        }

        public static IEnumerable<TestCaseData> CreateAseParameter_WithAseDbType_HasExpectedDbType_Cases()
        {
            yield return new TestCaseData("BigDateTime", DbType.DateTime, "DateTime");
            yield return new TestCaseData("BigInt", DbType.Int64, "BigInt");
            yield return new TestCaseData("Binary", DbType.Binary, "Binary");
            yield return new TestCaseData("Bit", DbType.Boolean, "Bit");
            yield return new TestCaseData("Char", DbType.AnsiStringFixedLength, "Char");
            yield return new TestCaseData("Date", DbType.Date, "Date");
            yield return new TestCaseData("DateTime", DbType.DateTime, "DateTime");
            yield return new TestCaseData("Decimal", DbType.Decimal, "Decimal");
            yield return new TestCaseData("Double", DbType.Double, "Double");
            yield return new TestCaseData("Image", DbType.Binary, "Image");
            yield return new TestCaseData("Integer", DbType.Int32, "Integer");
            yield return new TestCaseData("Money", DbType.Currency, "Money");
            yield return new TestCaseData("NChar", DbType.AnsiStringFixedLength, "NChar");
            yield return new TestCaseData("Numeric", DbType.VarNumeric, "Numeric");
            yield return new TestCaseData("NVarChar", DbType.AnsiString, "NVarChar");
            yield return new TestCaseData("Real", DbType.Single, "Real");
            yield return new TestCaseData("SmallDateTime", DbType.DateTime, "SmallDateTime");
            yield return new TestCaseData("SmallInt", DbType.Int16, "SmallInt");
            yield return new TestCaseData("SmallMoney", DbType.Currency, "SmallMoney");
            yield return new TestCaseData("Text", DbType.AnsiString, "Text");
            yield return new TestCaseData("Time", DbType.Time, "Time");
            yield return new TestCaseData("TimeStamp", DbType.Binary, "TimeStamp");
            yield return new TestCaseData("TinyInt", DbType.Byte, "TinyInt");
            yield return new TestCaseData("UniChar", DbType.StringFixedLength, "UniChar");
            yield return new TestCaseData("Unitext", DbType.String, "Unitext");
            yield return new TestCaseData("UniVarChar", DbType.String, "UniVarChar");
            yield return new TestCaseData("UnsignedBigInt", DbType.UInt64, "UnsignedBigInt");
            yield return new TestCaseData("UnsignedInt", DbType.UInt32, "UnsignedInt");
            yield return new TestCaseData("UnsignedSmallInt", DbType.UInt16, "UnsignedSmallInt");
            yield return new TestCaseData("Unsupported", DbType.Guid, "Unsupported");
            yield return new TestCaseData("VarBinary", DbType.Binary, "VarBinary");
            yield return new TestCaseData("VarChar", DbType.AnsiString, "VarChar");
        }

        [TestCaseSource(nameof(CreateAseParameter_WithDbType_HasExpectedAseDbType_Cases))]
        public void CreateAseParameter_WithDbType_HasExpectedAseDbType(DbType dbType, string expectedAseDbType, DbType expectedDbType)
        {
            var p = GetParameterProvider().GetParameter();
            p.DbType = dbType;

            AssertParameterTypes(p, expectedDbType, expectedAseDbType);
        }

        public static IEnumerable<TestCaseData> CreateAseParameter_WithDbType_HasExpectedAseDbType_Cases()
        {
            yield return new TestCaseData(DbType.AnsiString, AseDbType.VarChar.ToString(), DbType.AnsiString);
            yield return new TestCaseData(DbType.AnsiStringFixedLength, AseDbType.Char.ToString(), DbType.AnsiStringFixedLength);
            yield return new TestCaseData(DbType.Binary, AseDbType.Binary.ToString(), DbType.Binary);
            yield return new TestCaseData(DbType.Boolean, AseDbType.Bit.ToString(), DbType.Boolean);
            yield return new TestCaseData(DbType.Byte, AseDbType.TinyInt.ToString(), DbType.Byte);
            yield return new TestCaseData(DbType.Currency, AseDbType.Money.ToString(), DbType.Currency);
            yield return new TestCaseData(DbType.Date, AseDbType.Date.ToString(), DbType.Date);
            yield return new TestCaseData(DbType.DateTime, AseDbType.DateTime.ToString(), DbType.DateTime);
            yield return new TestCaseData(DbType.DateTime2, AseDbType.DateTime.ToString(), DbType.DateTime);
            yield return new TestCaseData(DbType.DateTimeOffset, AseDbType.Unsupported.ToString(), DbType.Guid); //DbType.Guid seems to be used as the "Unsupported" type
            yield return new TestCaseData(DbType.Decimal, AseDbType.Decimal.ToString(), DbType.Decimal);
            yield return new TestCaseData(DbType.Double, AseDbType.Double.ToString(), DbType.Double);
            yield return new TestCaseData(DbType.Guid, AseDbType.Unsupported.ToString(), DbType.Guid); //DbType.Guid seems to be used as the "Unsupported" type
            yield return new TestCaseData(DbType.Int16, AseDbType.SmallInt.ToString(), DbType.Int16);
            yield return new TestCaseData(DbType.Int32, AseDbType.Integer.ToString(), DbType.Int32);
            yield return new TestCaseData(DbType.Int64, AseDbType.BigInt.ToString(), DbType.Int64);
            yield return new TestCaseData(DbType.Object, AseDbType.Binary.ToString(), DbType.Binary);
            yield return new TestCaseData(DbType.SByte, AseDbType.Unsupported.ToString(), DbType.Guid); //DbType.Guid seems to be used as the "Unsupported" type
            yield return new TestCaseData(DbType.Single, AseDbType.Real.ToString(), DbType.Single);
            yield return new TestCaseData(DbType.String, AseDbType.UniVarChar.ToString(), DbType.String);
            yield return new TestCaseData(DbType.StringFixedLength, AseDbType.UniChar.ToString(), DbType.StringFixedLength);
            yield return new TestCaseData(DbType.Time, AseDbType.Time.ToString(), DbType.Time);
            yield return new TestCaseData(DbType.UInt16, AseDbType.UnsignedSmallInt.ToString(), DbType.UInt16);
            yield return new TestCaseData(DbType.UInt32, AseDbType.UnsignedInt.ToString(), DbType.UInt32);
            yield return new TestCaseData(DbType.UInt64, AseDbType.UnsignedBigInt.ToString(), DbType.UInt64);
            yield return new TestCaseData(DbType.VarNumeric, AseDbType.Numeric.ToString(), DbType.VarNumeric);
            yield return new TestCaseData(DbType.Xml, AseDbType.Unsupported.ToString(), DbType.Guid); //DbType.Guid seems to be used as the "Unsupported" type
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
            yield return new TestCaseData(new byte[] { 1 });
        }
    }
}
