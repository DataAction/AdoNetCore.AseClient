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
            
            DateTimeTestHelper.Insert_Parameter_VerifyResult(GetConnection, "insert_date_tests", "date_field", value);
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

        [TestCaseSource(nameof(Insert_Parameter_ExecuteScalar_AseDbType_Cases))]
        public void Insert_Parameter_ExecuteScalar_AseDbType(DateTime? value, string aseDbType, DateTime? expected)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "insert into [dbo].[insert_date_tests] (date_field) values (@date_field)";
                    var p = command.CreateParameter();
                    DateTimeTestHelper.SetAseDbType(p, aseDbType);
                    p.ParameterName = "@date_field";
                    p.Value = (object)value ?? DBNull.Value;
                    command.Parameters.Add(p);
                    command.ExecuteNonQuery();
                }
            }

            DateTimeTestHelper.Insert_Parameter_VerifyResult(GetConnection, "insert_date_tests", "date_field", expected);
        }

        public static IEnumerable<TestCaseData> Insert_Parameter_ExecuteScalar_AseDbType_Cases()
        {
            foreach (var testValue in DateTimeTestHelper.TestValues)
            {
                yield return new TestCaseData(testValue, "Date", testValue?.Date);
                yield return new TestCaseData(testValue, "DateTime", testValue?.Date);
                yield return new TestCaseData(testValue, "SmallDateTime", testValue?.Date);
                yield return new TestCaseData(testValue, "BigDateTime", testValue?.Date);
                yield return new TestCaseData(testValue, "Time", testValue?.Date);
            }
        }
    }
}
