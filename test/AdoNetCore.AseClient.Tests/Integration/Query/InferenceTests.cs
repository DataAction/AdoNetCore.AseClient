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
        private DbConnection GetConnection()
        {
            return Activator.CreateInstance<T>().GetConnection(ConnectionStrings.PooledUtf8);
        }

        [TestCaseSource(nameof(Select_UntypedParameter_ReturnsSameValue_Cases))]
        public void Select_UntypedParameter_ReturnsSameValue(object input, object expected)
        {
            using (var connection = GetConnection())
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
                    }
#endif
                    if (command is AseCommand cCore)
                    {
                        cCore.Parameters.Add(new AseParameter("@input", input));
                        Assert.AreEqual(AseDbType.Unsupported, cCore.Parameters[0].AseDbType);
                    }

                    Assert.AreEqual(expected, command.ExecuteScalar());
                }
            }
        }

        public static IEnumerable<TestCaseData> Select_UntypedParameter_ReturnsSameValue_Cases()
        {
            yield return new TestCaseData("master", "master");
            yield return new TestCaseData(new DateTime(2018, 11, 08, 11, 59, 59), new DateTime(2018, 11, 08, 11, 59, 59));
            yield return new TestCaseData(byte.MaxValue, byte.MaxValue);
            yield return new TestCaseData(short.MaxValue, short.MaxValue);
            yield return new TestCaseData(ushort.MaxValue, ushort.MaxValue);
            yield return new TestCaseData(int.MaxValue, int.MaxValue);
            yield return new TestCaseData(uint.MaxValue, uint.MaxValue);
            yield return new TestCaseData(long.MaxValue, long.MaxValue);
            yield return new TestCaseData(ulong.MaxValue, ulong.MaxValue);
            yield return new TestCaseData(1m, 1m);
            yield return new TestCaseData(float.MaxValue, float.MaxValue);
            yield return new TestCaseData(double.MaxValue, double.MaxValue);
            yield return new TestCaseData(true, true);

            if (typeof(T) == typeof(CoreFxConnectionProvider))
            {
                yield return new TestCaseData(new TimeSpan(11, 59, 59), new DateTime(1900, 01, 01, 11, 59, 59));
                var guid = Guid.NewGuid();
                yield return new TestCaseData(guid, guid.ToByteArray());
                yield return new TestCaseData(new byte[] { 0x01 }, new byte[] { 0x01 });
            }
        }
    }
}
