using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
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
    public class DateTests<T> where T : IConnectionProvider
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
                connection.Execute("create table [dbo].[insert_date_tests] (date_field date null)");
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("drop table [dbo].[insert_date_tests]");
            }
        }

        [TestCaseSource(nameof(Insert_Parameter_Cases))]
        public void Insert_Parameter_Dapper(DateTime? value)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@date_field", value, DbType.Date);
                connection.Execute("insert into [dbo].[insert_date_tests] (date_field) values (@date_field)", p);
            }

            using (var connection = GetConnection())
            {
                Assert.AreEqual(value, connection.QuerySingle<DateTime?>("select top 1 date_field from [dbo].[insert_date_tests]"));
            }
        }

        public static IEnumerable<TestCaseData> Insert_Parameter_Cases()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(new DateTime(0001, 01, 01));
            yield return new TestCaseData(new DateTime(2000, 11, 23));
            yield return new TestCaseData(new DateTime(2123, 11, 23));
            yield return new TestCaseData(new DateTime(3210, 11, 23));
            yield return new TestCaseData(new DateTime(9999, 12, 31));
        }
    }
}
