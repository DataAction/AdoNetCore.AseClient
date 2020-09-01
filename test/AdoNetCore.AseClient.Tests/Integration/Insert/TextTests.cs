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
    public class TextTests<T> where T : IConnectionProvider
    {
        private DbConnection GetConnection()
        {
            return Activator.CreateInstance<T>().GetConnection(ConnectionStrings.Pooled);
        }

        private const string SetUpSql = @"
create table [dbo].[insert_text_tests] (text_field text null)
create table [dbo].[insert_text_date_tests] (text_field text null, date_field datetime null)";

        private const string CleanUpSql = @"
IF EXISTS(SELECT 1 FROM sysobjects WHERE name = 'insert_text_tests')
BEGIN
    drop table [dbo].[insert_text_tests]
END

IF EXISTS(SELECT 1 FROM sysobjects WHERE name = 'insert_text_date_tests')
BEGIN
    drop table [dbo].[insert_text_date_tests]
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

        public static IEnumerable<DbType> Insert_Parameter_Types()
        {
            yield return DbType.AnsiString;
            yield return DbType.String;
        }

        public static IEnumerable<int> Insert_Parameter_Counts()
        {
            yield return 1;
            yield return 10;
            yield return 100;
            yield return 127;
            yield return 1000;
            yield return 4096;
            yield return 4097;
            yield return 8192;
            yield return 8193;
            yield return 10000;
            yield return 16384;
            yield return 16385;
            yield return 100000;
            yield return 1000000;
        }

        [Test]
        // Might have to increase the heap size with the command below
        // exec sp_configure 'heap memory per user',228352
        // http://infocenter.sybase.com/help/index.jsp?topic=/com.sybase.infocenter.dc31644.1570/html/sag2/sag258.htm
        public void Insert_Parameter_Dapper(
            [ValueSource(nameof(Insert_Parameter_Types))] DbType dbType,
            [ValueSource(nameof(Insert_Parameter_Counts))] int count)
        {
            var value = new string('1', count);
            using (var connection = GetConnection())
            {
                connection.Execute("set textsize 1000000");
                var p = new DynamicParameters();
                p.Add("@text_field", value, dbType);
                connection.Execute("insert into [dbo].[insert_text_tests] (text_field) values (@text_field)", p);
                var insertedLength = connection.QuerySingle<int>("select top 1 len(text_field) from [dbo].[insert_text_tests]");
                Assert.AreEqual(value.Length, insertedLength);
            }

            Insert_Parameter_VerifyResult(GetConnection, "insert_text_tests", "text_field", value);
        }

        [Test]
        // Might have to increase the heap size with the command below
        // exec sp_configure 'heap memory per user',228352
        // http://infocenter.sybase.com/help/index.jsp?topic=/com.sybase.infocenter.dc31644.1570/html/sag2/sag258.htm
        public void Insert_Parameter_With_Date_Dapper(
            [ValueSource(nameof(Insert_Parameter_Types))] DbType dbType,
            [ValueSource(nameof(Insert_Parameter_Counts))] int count)
        {
            var value = new string('1', count);
            var date = new DateTime(2001, 2, 3, 4, 5, 6);

            using (var connection = GetConnection())
            {
                connection.Execute("set textsize 1000000");
                var p = new DynamicParameters();
                p.Add("@text_field", value, dbType);
                p.Add("@date_field", date, DbType.DateTime);
                connection.Execute("insert into [dbo].[insert_text_date_tests] (text_field, date_field) values (@text_field, @date_field)", p);
                var insertedLength = connection.QuerySingle<int>("select top 1 len(text_field) from [dbo].[insert_text_date_tests]");
                Assert.AreEqual(value.Length, insertedLength);
            }

            Insert_Parameter_VerifyResult(GetConnection, "insert_text_date_tests", "text_field", value);
            Insert_Parameter_VerifyResult(GetConnection, "insert_text_date_tests", "date_field", date);
        }

        private void Insert_Parameter_VerifyResult<TColumn>(Func<DbConnection> getConnection, string table, string field, TColumn expected)
        {
            using (var connection = getConnection())
            {
                Assert.AreEqual(expected, connection.QuerySingle<TColumn>($"select top 1 {field} from [dbo].[{table}]"));
            }
        }
    }
}
