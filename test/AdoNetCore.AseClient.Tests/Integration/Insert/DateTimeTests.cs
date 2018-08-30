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
    public class DateTimeTests<T> where T : IConnectionProvider
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
                connection.Execute("create table [dbo].[insert_datetime_tests] (datetime_field datetime null)");
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("drop table [dbo].[insert_datetime_tests]");
            }
        }

        [TestCaseSource(nameof(Insert_Parameter_Cases))]
        public void Insert_Parameter_Dapper(string _, DateTime? value)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@datetime_field", value, DbType.DateTime);
                connection.Execute("insert into [dbo].[insert_datetime_tests] (datetime_field) values (@datetime_field)", p);
            }
            using (var connection = GetConnection())
            {
                Assert.AreEqual(value, connection.QuerySingle<DateTime?>("select top 1 datetime_field from [dbo].[insert_datetime_tests]"));
            }
        }

        public static IEnumerable<TestCaseData> Insert_Parameter_Cases()
        {
            yield return new TestCaseData("null", null);
            yield return new TestCaseData("2000", new DateTime(2000, 11, 12, 21, 14, 15, 166));
            yield return new TestCaseData("2001", new DateTime(2001, 10, 12, 21, 14, 15, 166));
            yield return new TestCaseData("2098", new DateTime(2098, 10, 12, 21, 14, 15, 906));
            //0001-01-01 results in "Arithmetic overflow during implicit conversion of BIGDATETIME value 'Jan  1 0001 12:00AM' to a DATETIME field ." on SAP Driver
            /*yield return new TestCaseData("0001_0", new DateTime(0001, 1, 1, 0, 0, 0));
            yield return new TestCaseData("0001_1", new DateTime(0001, 1, 1, 23, 59, 59));
            yield return new TestCaseData("0001_2", new DateTime(0001, 1, 1, 23, 59, 59, 997));
            yield return new TestCaseData("0001_3", new DateTime(0001, 12, 31, 0, 0, 0));
            yield return new TestCaseData("0001_4", new DateTime(0001, 12, 31, 23, 59, 59));
            yield return new TestCaseData("0001_5", new DateTime(0001, 12, 31, 23, 59, 59, 997));*/
            yield return new TestCaseData("1753_0", new DateTime(1753, 1, 1, 0, 0, 0));
            yield return new TestCaseData("1753_1", new DateTime(1753, 1, 1, 23, 59, 59));
            yield return new TestCaseData("1753_2", new DateTime(1753, 1, 1, 23, 59, 59, 996));
            yield return new TestCaseData("1753_3", new DateTime(1753, 12, 31, 0, 0, 0));
            yield return new TestCaseData("1753_4", new DateTime(1753, 12, 31, 23, 59, 59));
            yield return new TestCaseData("1753_5", new DateTime(1753, 12, 31, 23, 59, 59, 996));
            yield return new TestCaseData("1900_0", new DateTime(1900, 1, 1, 0, 0, 0));
            yield return new TestCaseData("1900_1", new DateTime(1900, 1, 1, 23, 59, 59));
            yield return new TestCaseData("1900_2", new DateTime(1900, 1, 1, 23, 59, 59, 996));
            yield return new TestCaseData("1900_3", new DateTime(1900, 12, 31, 0, 0, 0));
            yield return new TestCaseData("1900_4", new DateTime(1900, 12, 31, 23, 59, 59));
            yield return new TestCaseData("1900_5", new DateTime(1900, 12, 31, 23, 59, 59, 996));
            yield return new TestCaseData("9999_0", new DateTime(9999, 01, 01, 0, 0, 0));
            yield return new TestCaseData("9999_1", new DateTime(9999, 01, 01, 23, 59, 59));
            yield return new TestCaseData("9999_2", new DateTime(9999, 01, 01, 23, 59, 59, 996));
            yield return new TestCaseData("9999_3", new DateTime(9999, 12, 31, 0, 0, 0));
            yield return new TestCaseData("9999_4", new DateTime(9999, 12, 31, 23, 59, 59));
            yield return new TestCaseData("9999_5", new DateTime(9999, 12, 31, 23, 59, 59, 996));
        }
    }
}
