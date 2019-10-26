using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using AdoNetCore.AseClient.Internal;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Insert
{
    [Category("basic")]
#if NET_FRAMEWORK
    [TestFixture(typeof(SapConnectionProvider), Explicit = true, Reason = "SAP AseClient tests are run for compatibility purposes.")]
#endif
    [TestFixture(typeof(CoreFxConnectionProvider))]
    public class BinaryTests<T> where T : IConnectionProvider
    {
        private DbConnection GetConnection()
        {
            return Activator.CreateInstance<T>().GetConnection(ConnectionStrings.Pooled);
        }

        private const string SetUpSql = @"create table [dbo].[insert_image_tests] (image_field image null)";
        private const string CleanUpSql = @"IF EXISTS(SELECT 1 FROM sysobjects WHERE name = 'insert_image_tests')
BEGIN
    drop table [dbo].[insert_image_tests]
END";

        [SetUp]
        public void Setup()
        {
            Logger.Enable();

            using (var connection = GetConnection())
            {
                connection.Execute(CleanUpSql);
                connection.Execute(SetUpSql);
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

        public static IEnumerable<TestCaseData> Insert_Parameter_Cases()
        {
            yield return new TestCaseData(1);
            yield return new TestCaseData(10);
            yield return new TestCaseData(100);
            yield return new TestCaseData(127);
            yield return new TestCaseData(1000);
            yield return new TestCaseData(8192);
            yield return new TestCaseData(8193);
            yield return new TestCaseData(10000);
            yield return new TestCaseData(16384);
            yield return new TestCaseData(16385);
            yield return new TestCaseData(100000);
            yield return new TestCaseData(1000000);
        }

        [TestCaseSource(nameof(Insert_Parameter_Cases))]
        public void Insert_Parameter_Dapper(int count)
        {
            var value = new byte[count];
            using (var connection = GetConnection())
            {
                connection.Execute("set textsize 1000000");
                var p = new DynamicParameters();
                p.Add("@image_field", value, DbType.Binary);
                connection.Execute("insert into [dbo].[insert_image_tests] (image_field) values (@image_field)", p);
                var insertedLength = connection.QuerySingle<int>("select top 1 datalength(image_field) from [dbo].[insert_image_tests]");
                Assert.AreEqual(value.Length, insertedLength);
            }

            Insert_Parameter_VerifyResult(GetConnection, "insert_image_tests", "image_field", value);
        }

        private void Insert_Parameter_VerifyResult(Func<DbConnection> getConnection, string table, string field, byte[] expected)
        {
            using (var connection = getConnection())
            {
                Assert.AreEqual(expected, connection.QuerySingle<byte[]>($"select top 1 {field} from [dbo].[{table}]"));
            }
        }
    }
}
