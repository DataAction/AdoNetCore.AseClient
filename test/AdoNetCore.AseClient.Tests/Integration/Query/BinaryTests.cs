using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Query
{
    [Category("basic")]
#if NET_FRAMEWORK
    [TestFixture(typeof(SapConnectionProvider))]
#endif
    [TestFixture(typeof(CoreFxConnectionProvider))]
    public class BinaryTests<T> where T : IConnectionProvider
    {
        private DbConnection GetConnection()
        {
            return Activator.CreateInstance<T>().GetConnection(ConnectionStrings.Pooled);
        }

        [TestCaseSource(nameof(SelectLiteral_Cases))]
        public void SelectLiteral_Dapper(string query, byte[] expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<byte[]>(query));
            }
        }

        [TestCaseSource(nameof(SelectLiteral_Cases))]
        public void SelectLiteral_ExecuteScalar(string query, object expected)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    Assert.AreEqual(expected ?? DBNull.Value, command.ExecuteScalar());
                }
            }
        }

        public static IEnumerable<TestCaseData> SelectLiteral_Cases()
        {
            yield return new TestCaseData("select convert(image, null)", null);
            yield return new TestCaseData("select convert(image, 0xffffffff)", new byte[] {0xff, 0xff, 0xff, 0xff});
        }

        [TestCaseSource(nameof(TestParameter_Cases))]
        public void SelectParameter_Dapper(object parameterValue, object expected)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@expected", expected, DbType.Binary);
                Assert.AreEqual(expected, connection.ExecuteScalar<byte[]>("select @expected", p));
            }
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
                    p.DbType = DbType.Binary;
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
            yield return new TestCaseData(new byte[0], new byte[] {0});
            yield return new TestCaseData(new byte[] {0xff}, new byte[] {0xff});
        }
    }
}
