using System;
using System.Collections.Generic;
using System.Data.Common;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Query
{
    [Category("basic")]
#if NET_FRAMEWORK
    [TestFixture(typeof(SapConnectionProvider), Explicit = true, Reason = "SAP AseClient tests are run for compatibility purposes.")]
#endif
    [TestFixture(typeof(CoreFxConnectionProvider))]
    public class AseImageTests<T> where T : IConnectionProvider
    {
        private DbConnection GetConnection()
        {
            return Activator.CreateInstance<T>().GetConnection(ConnectionStrings.Pooled);
        }

        [TestCaseSource(nameof(TestParameter_Cases))]
        public void SelectParameter_ExecuteScalar(object parameterValue, object expected)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select @expected";
                    var p = command.CreateParameter();
#if NET_FRAMEWORK
                    if (p is Sybase.Data.AseClient.AseParameter pSap)
                    {
                        pSap.AseDbType = Sybase.Data.AseClient.AseDbType.Image;
                    }
#endif
                    if (p is AseParameter pCore)
                    {
                        pCore.AseDbType = AseDbType.Image;
                    }

                    p.ParameterName = "@expected";
                    p.Value = parameterValue ?? DBNull.Value;
                    command.Parameters.Add(p);
                    Assert.AreEqual(expected ?? DBNull.Value, command.ExecuteScalar());
                }
            }
        }

        public static IEnumerable<TestCaseData> TestParameter_Cases()
        {
            yield return new TestCaseData(null, null);
            yield return new TestCaseData(new byte[0], null);
            yield return new TestCaseData(new byte[] { 0xff }, new byte[] { 0xff });
        }
    }
}
