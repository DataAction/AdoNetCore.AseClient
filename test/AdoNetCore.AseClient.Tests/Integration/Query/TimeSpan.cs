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
    public class TimeSpanTests
    {
        private IDbConnection GetConnection()
        {
            Logger.Enable();
            return new AseConnection(ConnectionStrings.Pooled);
        }
        
        [Test]
        public void SelectLiteral_ExecuteScalar()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Cast('12:12:12' as time)";
                    Assert.AreEqual(new DateTime(1900, 01, 01, 12, 12, 12), command.ExecuteScalar());
                }
            }
        }

        [Test]
        public void SelectLiteral_ExecuteDataReader()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Cast('12:12:12' as time)";
                    var reader = command.ExecuteReader();
                    reader.Read();

                    Assert.AreEqual(new DateTime(1900, 01, 01, 12, 12, 12), reader.GetDateTime(0));
                    Assert.AreEqual(new TimeSpan(12, 12, 12), ((AseDataReader)reader).GetTimeSpan(0));
                }
            }
        }
        
        [TestCaseSource(nameof(TestParameter_Cases))]
        public void TestParameter(DateTime expected, object parameterValue)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT @p";

                    var p = command.CreateParameter();
                    p.DbType = DbType.Time;
                    p.ParameterName = "@p";
                    p.Value = parameterValue;
                    command.Parameters.Add(p);

                    Assert.AreEqual(expected, command.ExecuteScalar());
                }
            }
        }

        public static IEnumerable<TestCaseData> TestParameter_Cases()
        {
            yield return new TestCaseData(new DateTime(1900, 01, 01, 12, 12, 12), new TimeSpan(12, 12, 12));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 12, 12, 12), new DateTime(1900, 01, 01, 12, 12, 12));
        }
    }
}
