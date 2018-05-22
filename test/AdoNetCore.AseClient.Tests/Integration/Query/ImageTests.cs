using System;
using System.Collections.Generic;
using System.Data;
using AdoNetCore.AseClient.Internal;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Query
{
    [TestFixture]
    [Category("basic")]
    public class ImageTests
    {
        private IDbConnection GetConnection()
        {
            Logger.Enable();
            return new AseConnection(ConnectionStrings.Pooled);
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
    }
}
