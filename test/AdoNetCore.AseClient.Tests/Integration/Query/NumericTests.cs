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
    [TestFixture(typeof(SapConnectionProvider), Explicit = true, Reason = "SAP AseClient tests are run for compatibility purposes.")]
#endif
    [TestFixture(typeof(CoreFxConnectionProvider))]
    public class NumericTests<T> where T : IConnectionProvider
    {
        private DbConnection GetConnection()
        {
            return Activator.CreateInstance<T>().GetConnection(ConnectionStrings.PooledUtf8);
        }

        [SetUp]
        public void Setup()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("create table [dbo].[decimal_test_table] (value decimal(18,4))");
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("drop table [dbo].[decimal_test_table]");
            }
        }

        public static IEnumerable<TestCaseData> SelectNumeric_FromTable_Cases()
        {
            yield return new TestCaseData(1m);
            yield return new TestCaseData(2819.0444m);
            yield return new TestCaseData(12345678901234.9999m);
            yield return new TestCaseData(-12345678901234.9999m);
        }

        [TestCaseSource(nameof(SelectNumeric_FromTable_Cases))]
        public void SelectNumeric_FromTable(decimal input)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@input", input, DbType.Decimal);
                connection.Execute("insert into [dbo].[decimal_test_table] (value) values (@input)", p);
            }

            using (var connection = GetConnection())
            {
                Assert.AreEqual(input, connection.QuerySingle<decimal>("select top 1 value from [dbo].[decimal_test_table]"));
            }
        }
    }
}
