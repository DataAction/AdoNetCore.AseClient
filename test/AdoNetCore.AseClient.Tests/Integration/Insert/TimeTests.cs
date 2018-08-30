using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Insert
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

        [SetUp]
        public void Setup()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("create table [dbo].[insert_time_tests] (time_field time null)");
            }
        }
        [TearDown]
        public void TearDown()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("drop table [dbo].[insert_time_tests]");
            }
        }
        [TestCaseSource(nameof(Insert_Parameter_Cases))]
        public void Insert_Parameter_Dapper(DateTime? value)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@time_field", value, DbType.Time);
                connection.Execute("insert into [dbo].[insert_time_tests] (time_field) values (@time_field)", p);
            }
            using (var connection = GetConnection())
            {
                Assert.AreEqual(value, connection.QuerySingle<DateTime?>("select top 1 time_field from [dbo].[insert_time_tests]"));
            }
        }
        public static IEnumerable<TestCaseData> Insert_Parameter_Cases()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(new DateTime(1900, 01, 01, 0, 0, 0, 0));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 0, 44, 33, 876));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 12, 12, 12));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 14, 44, 33, 233));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 22, 44, 33, 0));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 23, 59, 59, 996));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 23, 59, 59, 996));
            yield return new TestCaseData(new DateTime(1900, 01, 01, 9, 44, 33, 886));
        }
    }
}
