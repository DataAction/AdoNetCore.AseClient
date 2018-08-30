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
    public class TimeTests<T> where T : IConnectionProvider
    {
        private DbConnection GetConnection()
        {
            return Activator.CreateInstance<T>().GetConnection(ConnectionStrings.Pooled);
        }

        [TestCaseSource(nameof(SelectLiteral_Cases))]
        public void SelectLiteral_Dapper(string query, DateTime? expected)
        {
            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.ExecuteScalar<DateTime?>(query));
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
            yield return new TestCaseData("select convert(time, null)", null);
            yield return new TestCaseData("select convert(time, '12:12:12')", new DateTime(1900, 01, 01, 12, 12, 12));
            yield return new TestCaseData("select convert(time, '00:00:00')", new DateTime(1900, 01, 01, 0, 0, 0, 0));
            yield return new TestCaseData("select convert(time, '23:59:59.997')", new DateTime(1900, 01, 01, 23, 59, 59, 997));
        }

        

        [Test]
        public void SelectLiteral_ExecuteDataReader()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select convert(time, '12:12:12')";
                    var reader = command.ExecuteReader();
                    reader.Read();

                    Assert.AreEqual(new DateTime(1900, 01, 01, 12, 12, 12), reader.GetDateTime(0));
#if NET_FRAMEWORK
                    if (reader is Sybase.Data.AseClient.AseDataReader readerSap)
                    {
                        Assert.AreEqual(new TimeSpan(12, 12, 12), readerSap.GetTimeSpan(0));
                    }
#endif
                    if (reader is AseDataReader readerCore)
                    {
                        Assert.AreEqual(new TimeSpan(12, 12, 12), readerCore.GetTimeSpan(0));
                    }
                }
            }
        }

        [TestCaseSource(nameof(TestParameter_Cases))]
        public void SelectParameter_Dapper(object parameterValue, object expected)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@expected", expected, DbType.Time);
                Assert.AreEqual(expected, connection.ExecuteScalar<DateTime?>("select @expected", p));
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
                    p.DbType = DbType.Time;
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
            yield return new TestCaseData(new DateTime(1900, 01, 01, 0, 0, 0, 0), new DateTime(1900, 01, 01, 0, 0, 0, 0));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 0, 44, 33, 876), new DateTime(1900, 01, 01, 0, 44, 33, 876));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 12, 12, 12), new DateTime(1900, 01, 01, 12, 12, 12));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 14, 44, 33, 233), new DateTime(1900, 01, 01, 14, 44, 33, 233));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 22, 44, 33, 0), new DateTime(1900, 01, 01, 22, 44, 33, 0));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 23, 59, 59, 996), new DateTime(1900, 01, 01, 23, 59, 59, 996));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 23, 59, 59, 997), new DateTime(1900, 01, 01, 23, 59, 59, 997));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 9, 44, 33, 886), new DateTime(1900, 01, 01, 9, 44, 33, 886));

            if (typeof(T) == typeof(CoreFxConnectionProvider))
            {
                yield return new TestCaseData(new TimeSpan(0, 0, 0, 0, 0), new DateTime(1900, 01, 01, 0, 0, 0, 0));
                yield return new TestCaseData(new TimeSpan(0, 23, 59, 59, 997), new DateTime(1900, 01, 01, 23, 59, 59, 996));
                yield return new TestCaseData(new TimeSpan(12, 12, 12), new DateTime(1900, 01, 01, 12, 12, 12));
            }
        }
    }
}
