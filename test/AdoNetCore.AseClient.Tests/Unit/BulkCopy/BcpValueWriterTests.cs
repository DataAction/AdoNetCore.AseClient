using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Internal.BulkCopy;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit.BulkCopy
{
    [TestFixture]
    public class BcpValueWriterTests
    {
        private static BcpColumnMetadata MakeFixed(TdsDataType type)
        {
            return new BcpColumnMetadata { DataType = type };
        }

        private static BcpColumnMetadata MakeVariable(TdsDataType type, int length)
        {
            return new BcpColumnMetadata { DataType = type, Length = length };
        }

        #region Fixed-Length Types

        [Test]
        public void WriteFixedValue_Bit_True()
        {
            using (var ms = new MemoryStream())
            {
                BcpValueWriter.WriteFixedValue(ms, true, MakeFixed(TdsDataType.TDS_BIT), Encoding.ASCII);
                Assert.AreEqual(new byte[] { 1 }, ms.ToArray());
            }
        }

        [Test]
        public void WriteFixedValue_Bit_False()
        {
            using (var ms = new MemoryStream())
            {
                BcpValueWriter.WriteFixedValue(ms, false, MakeFixed(TdsDataType.TDS_BIT), Encoding.ASCII);
                Assert.AreEqual(new byte[] { 0 }, ms.ToArray());
            }
        }

        [Test]
        public void WriteFixedValue_Int1()
        {
            using (var ms = new MemoryStream())
            {
                BcpValueWriter.WriteFixedValue(ms, (byte)42, MakeFixed(TdsDataType.TDS_INT1), Encoding.ASCII);
                Assert.AreEqual(new byte[] { 42 }, ms.ToArray());
            }
        }

        [Test]
        public void WriteFixedValue_Int2()
        {
            using (var ms = new MemoryStream())
            {
                BcpValueWriter.WriteFixedValue(ms, (short)1234, MakeFixed(TdsDataType.TDS_INT2), Encoding.ASCII);
                Assert.AreEqual(BitConverter.GetBytes((short)1234), ms.ToArray());
            }
        }

        [Test]
        public void WriteFixedValue_Int4()
        {
            using (var ms = new MemoryStream())
            {
                BcpValueWriter.WriteFixedValue(ms, 123456, MakeFixed(TdsDataType.TDS_INT4), Encoding.ASCII);
                Assert.AreEqual(BitConverter.GetBytes(123456), ms.ToArray());
            }
        }

        [Test]
        public void WriteFixedValue_Int8()
        {
            using (var ms = new MemoryStream())
            {
                BcpValueWriter.WriteFixedValue(ms, 123456789L, MakeFixed(TdsDataType.TDS_INT8), Encoding.ASCII);
                Assert.AreEqual(BitConverter.GetBytes(123456789L), ms.ToArray());
            }
        }

        [Test]
        public void WriteFixedValue_Flt4()
        {
            using (var ms = new MemoryStream())
            {
                BcpValueWriter.WriteFixedValue(ms, 3.14f, MakeFixed(TdsDataType.TDS_FLT4), Encoding.ASCII);
                Assert.AreEqual(BitConverter.GetBytes(3.14f), ms.ToArray());
            }
        }

        [Test]
        public void WriteFixedValue_Flt8()
        {
            using (var ms = new MemoryStream())
            {
                BcpValueWriter.WriteFixedValue(ms, 3.14159265, MakeFixed(TdsDataType.TDS_FLT8), Encoding.ASCII);
                Assert.AreEqual(BitConverter.GetBytes(3.14159265), ms.ToArray());
            }
        }

        [Test]
        public void WriteFixedValue_Null_ZeroFills()
        {
            using (var ms = new MemoryStream())
            {
                BcpValueWriter.WriteFixedValue(ms, DBNull.Value, MakeFixed(TdsDataType.TDS_INT4), Encoding.ASCII);
                Assert.AreEqual(new byte[] { 0, 0, 0, 0 }, ms.ToArray());
            }
        }

        [Test]
        public void WriteFixedValue_Null_Int8_ZeroFills()
        {
            using (var ms = new MemoryStream())
            {
                BcpValueWriter.WriteFixedValue(ms, null, MakeFixed(TdsDataType.TDS_INT8), Encoding.ASCII);
                Assert.AreEqual(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, ms.ToArray());
            }
        }

        [Test]
        public void WriteFixedValue_UInt2()
        {
            using (var ms = new MemoryStream())
            {
                BcpValueWriter.WriteFixedValue(ms, (ushort)65535, MakeFixed(TdsDataType.TDS_UINT2), Encoding.ASCII);
                Assert.AreEqual(BitConverter.GetBytes((ushort)65535), ms.ToArray());
            }
        }

        [Test]
        public void WriteFixedValue_UInt4()
        {
            using (var ms = new MemoryStream())
            {
                BcpValueWriter.WriteFixedValue(ms, 4294967295u, MakeFixed(TdsDataType.TDS_UINT4), Encoding.ASCII);
                Assert.AreEqual(BitConverter.GetBytes(4294967295u), ms.ToArray());
            }
        }

        [Test]
        public void WriteFixedValue_UInt8()
        {
            using (var ms = new MemoryStream())
            {
                BcpValueWriter.WriteFixedValue(ms, (ulong)18446744073709551615, MakeFixed(TdsDataType.TDS_UINT8), Encoding.ASCII);
                Assert.AreEqual(BitConverter.GetBytes((ulong)18446744073709551615), ms.ToArray());
            }
        }

        #endregion

        #region Variable-Length Types

        [Test]
        public void WriteVariableValue_Varchar()
        {
            using (var ms = new MemoryStream())
            {
                var len = BcpValueWriter.WriteVariableValue(ms, "hello", MakeVariable(TdsDataType.TDS_VARCHAR, 255), Encoding.ASCII);
                Assert.AreEqual(5, len);
                Assert.AreEqual(Encoding.ASCII.GetBytes("hello"), ms.ToArray());
            }
        }

        [Test]
        public void WriteVariableValue_Varchar_Truncates()
        {
            using (var ms = new MemoryStream())
            {
                var len = BcpValueWriter.WriteVariableValue(ms, "hello world", MakeVariable(TdsDataType.TDS_VARCHAR, 5), Encoding.ASCII);
                Assert.AreEqual(5, len);
                Assert.AreEqual(Encoding.ASCII.GetBytes("hello"), ms.ToArray());
            }
        }

        [Test]
        public void WriteVariableValue_Null_ReturnsZero()
        {
            using (var ms = new MemoryStream())
            {
                var len = BcpValueWriter.WriteVariableValue(ms, DBNull.Value, MakeVariable(TdsDataType.TDS_VARCHAR, 255), Encoding.ASCII);
                Assert.AreEqual(0, len);
                Assert.AreEqual(0, ms.Length);
            }
        }

        [Test]
        public void WriteVariableValue_Binary()
        {
            var data = new byte[] { 0x01, 0x02, 0x03 };
            using (var ms = new MemoryStream())
            {
                var len = BcpValueWriter.WriteVariableValue(ms, data, MakeVariable(TdsDataType.TDS_BINARY, 10), Encoding.ASCII);
                Assert.AreEqual(3, len);
                Assert.AreEqual(data, ms.ToArray());
            }
        }

        [Test]
        public void WriteVariableValue_IntN_4Byte()
        {
            using (var ms = new MemoryStream())
            {
                var len = BcpValueWriter.WriteVariableValue(ms, 42, MakeVariable(TdsDataType.TDS_INTN, 4), Encoding.ASCII);
                Assert.AreEqual(4, len);
                Assert.AreEqual(BitConverter.GetBytes(42), ms.ToArray());
            }
        }

        [Test]
        public void WriteVariableValue_IntN_8Byte()
        {
            using (var ms = new MemoryStream())
            {
                var len = BcpValueWriter.WriteVariableValue(ms, 42L, MakeVariable(TdsDataType.TDS_INTN, 8), Encoding.ASCII);
                Assert.AreEqual(8, len);
                Assert.AreEqual(BitConverter.GetBytes(42L), ms.ToArray());
            }
        }

        [Test]
        public void WriteVariableValue_FloatN_4Byte()
        {
            using (var ms = new MemoryStream())
            {
                var len = BcpValueWriter.WriteVariableValue(ms, 3.14f, MakeVariable(TdsDataType.TDS_FLTN, 4), Encoding.ASCII);
                Assert.AreEqual(4, len);
                Assert.AreEqual(BitConverter.GetBytes(3.14f), ms.ToArray());
            }
        }

        [Test]
        public void WriteVariableValue_FloatN_8Byte()
        {
            using (var ms = new MemoryStream())
            {
                var len = BcpValueWriter.WriteVariableValue(ms, 3.14, MakeVariable(TdsDataType.TDS_FLTN, 8), Encoding.ASCII);
                Assert.AreEqual(8, len);
                Assert.AreEqual(BitConverter.GetBytes(3.14), ms.ToArray());
            }
        }

        [Test]
        public void WriteVariableValue_LongChar()
        {
            using (var ms = new MemoryStream())
            {
                var len = BcpValueWriter.WriteVariableValue(ms, "test string", MakeVariable(TdsDataType.TDS_LONGCHAR, 1000), Encoding.ASCII);
                Assert.AreEqual(11, len);
                Assert.AreEqual(Encoding.ASCII.GetBytes("test string"), ms.ToArray());
            }
        }

        [Test]
        public void WriteVariableValue_VarBinary()
        {
            var data = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            using (var ms = new MemoryStream())
            {
                var len = BcpValueWriter.WriteVariableValue(ms, data, MakeVariable(TdsDataType.TDS_VARBINARY, 100), Encoding.ASCII);
                Assert.AreEqual(4, len);
                Assert.AreEqual(data, ms.ToArray());
            }
        }

        #endregion
    }
}
