using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Query
{
    [Category("basic")]
#if NET_FRAMEWORK
    [TestFixture(typeof(SapConnectionProvider))]
#endif
    [TestFixture(typeof(CoreFxConnectionProvider))]
    public class InferenceTests<T> where T : IConnectionProvider
    {
        private DbConnection GetConnection(bool aseDecimal)
        {
            return Activator.CreateInstance<T>().GetConnection(aseDecimal ? ConnectionStrings.AseDecimalOn: ConnectionStrings.PooledUtf8);
        }

        [TestCaseSource(nameof(Select_UntypedParameter_ReturnsSameValue_Cases))]
        public void Select_UntypedParameter_ReturnsSameValue(object input, object expected, int aseDbType)
        {
            using (var connection = GetConnection(input is AseDecimal))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select @input";
                    command.CommandType = CommandType.Text;
#if NET_FRAMEWORK
                    if (command is Sybase.Data.AseClient.AseCommand cSap)
                    {
                        cSap.Parameters.Add(new Sybase.Data.AseClient.AseParameter("@input", input));
                        Assert.AreEqual(Sybase.Data.AseClient.AseDbType.Unsupported, cSap.Parameters[0].AseDbType);
                        Assert.AreEqual(expected, command.ExecuteScalar());
                        Assert.AreEqual((Sybase.Data.AseClient.AseDbType) aseDbType, cSap.Parameters[0].AseDbType);
                    }
#endif
                    if (command is AseCommand cCore)
                    {
                        cCore.Parameters.Add(new AseParameter("@input", input));
                        Assert.AreEqual(AseDbType.Unsupported, cCore.Parameters[0].AseDbType);
                        Assert.AreEqual(expected, command.ExecuteScalar());
                        Assert.AreEqual((AseDbType) aseDbType, cCore.Parameters[0].AseDbType);
                    }
                }
            }
        }

        public static IEnumerable<TestCaseData> Select_UntypedParameter_ReturnsSameValue_Cases()
        {
            yield return new TestCaseData("master", "master", AseDbType.VarChar);
            yield return new TestCaseData('a', "a", AseDbType.VarChar);
            yield return new TestCaseData(new DateTime(2018, 11, 08, 11, 59, 59), new DateTime(2018, 11, 08, 11, 59, 59), AseDbType.DateTime);
            yield return new TestCaseData(byte.MaxValue, byte.MaxValue, AseDbType.TinyInt);
            yield return new TestCaseData(short.MaxValue, short.MaxValue, AseDbType.SmallInt);
            yield return new TestCaseData(ushort.MaxValue, ushort.MaxValue, AseDbType.UnsignedSmallInt);
            yield return new TestCaseData(int.MaxValue, int.MaxValue, AseDbType.Integer);
            yield return new TestCaseData(uint.MaxValue, uint.MaxValue, AseDbType.UnsignedInt);
            yield return new TestCaseData(long.MaxValue, long.MaxValue, AseDbType.BigInt);
            yield return new TestCaseData(ulong.MaxValue, ulong.MaxValue, AseDbType.UnsignedBigInt);
            yield return new TestCaseData(1.111m, 1.111m, AseDbType.Numeric);
            yield return new TestCaseData(float.MaxValue, float.MaxValue, AseDbType.Real);
            yield return new TestCaseData(double.MaxValue, double.MaxValue, AseDbType.Double);
            yield return new TestCaseData(true, true, AseDbType.Bit);

            if (typeof(T) == typeof(CoreFxConnectionProvider))
            {
                yield return new TestCaseData(new TimeSpan(11, 59, 59), new DateTime(1900, 01, 01, 11, 59, 59), AseDbType.DateTime);
                var guid = Guid.NewGuid();
                yield return new TestCaseData(guid, guid.ToByteArray(), AseDbType.VarBinary);
                yield return new TestCaseData(new byte[] { 0x01 }, new byte[] { 0x01 }, AseDbType.VarBinary);
                yield return new TestCaseData(sbyte.MaxValue, sbyte.MaxValue, AseDbType.SmallInt);
                yield return new TestCaseData(new [] {'a'}, "a", AseDbType.VarChar);
                // reference driver throws NotSupportedException
                yield return new TestCaseData(new AseDecimal(2.111m), new AseDecimal(2.111m), AseDbType.Decimal);
            }
        }
    }
}
