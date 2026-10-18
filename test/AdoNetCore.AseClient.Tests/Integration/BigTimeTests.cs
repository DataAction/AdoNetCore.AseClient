using System;
using System.Data.Common;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [Category("basic")]
#if NET_FRAMEWORK
    [TestFixture(typeof(SapConnectionProvider), Explicit = true, Reason = "SAP AseClient tests are run for compatibility purposes.")]
#endif
    [TestFixture(typeof(CoreFxConnectionProvider))]
    public class BigTimeTests<T> where T : IConnectionProvider
    {
        private const string CleanUpSql =
@"IF EXISTS(SELECT 1 FROM sysobjects WHERE name = 'bigtime_tests')
BEGIN
    drop table [dbo].[bigtime_tests]
END";

        private DbConnection GetConnection()
        {
            return Activator.CreateInstance<T>().GetConnection(ConnectionStrings.Pooled);
        }

        [SetUp]
        public void Setup()
        {
            using (var connection = GetConnection())
            {
                connection.Execute(CleanUpSql);
                connection.Execute("create table [dbo].[bigtime_tests] (id int not null, time_field bigtime null)");
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var connection = GetConnection())
            {
                connection.Execute(CleanUpSql);
            }
        }

        [TestCase("00:00:00.000000", 0)]
        [TestCase("13:45:30.123456", 1234560)]
        [TestCase("23:59:59.999999", 9999990)]
        public void Select_BigTime_ReadsTheTimeOfDayAtMicrosecondResolution(string text, long fractionTicks)
        {
            using (var connection = GetConnection())
            {
                connection.Execute($"insert into [dbo].[bigtime_tests] (id, time_field) values (1, '{text}')");
                connection.Execute("insert into [dbo].[bigtime_tests] (id, time_field) values (2, null)");

                var value = connection.QuerySingle<DateTime?>("select time_field from [dbo].[bigtime_tests] where id = 1");
                var parts = text.Split(':', '.');
                var expected = new DateTime(1900, 1, 1, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2])).AddTicks(fractionTicks);
                Assert.AreEqual(expected, value);

                var missing = connection.QuerySingle<DateTime?>("select time_field from [dbo].[bigtime_tests] where id = 2");
                Assert.IsNull(missing);
            }
        }
    }
}
