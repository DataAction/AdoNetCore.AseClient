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
    [TestFixture(typeof(SapConnectionProvider), Explicit = true, Reason = "SAP AseClient tests are run for compatibility purposes.")]
#endif
    [TestFixture(typeof(CoreFxConnectionProvider))]
    public class TimeTests<T> where T : IConnectionProvider
    {
        private const string CleanUpSql =
@"IF EXISTS(SELECT 1 FROM sysobjects WHERE name = 'insert_time_tests')
BEGIN
    drop table [dbo].[insert_time_tests]
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
                connection.Execute("create table [dbo].[insert_time_tests] (time_field time null)");
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

        [TestCaseSource(nameof(Insert_Parameter_Cases))]
        public void Insert_Parameter_Dapper(DateTime? value)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@time_field", value, DbType.Time);
                connection.Execute("insert into [dbo].[insert_time_tests] (time_field) values (@time_field)", p);
            }

            DateTimeTestHelper.Insert_Parameter_VerifyResult(GetConnection, "insert_time_tests", "time_field", value);
        }

        [TestCaseSource(nameof(Insert_Parameter_ExecuteScalar_AseDbType_Cases))]
        public void Insert_Parameter_ExecuteScalar_AseDbType(DateTime? value, string aseDbType, DateTime? expected)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "insert into [dbo].[insert_time_tests] (time_field) values (@time_field)";
                    var p = command.CreateParameter();
                    DateTimeTestHelper.SetAseDbType(p, aseDbType);
                    p.ParameterName = "@time_field";
                    p.Value = (object)value ?? DBNull.Value;
                    command.Parameters.Add(p);
                    command.ExecuteNonQuery();
                }
            }

            DateTimeTestHelper.Insert_Parameter_VerifyResult(GetConnection, "insert_time_tests", "time_field", expected);
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

        public static IEnumerable<TestCaseData> Insert_Parameter_ExecuteScalar_AseDbType_Cases()
        {
            foreach (var testValue in DateTimeTestHelper.TestValues)
            {
                //yield return new TestCaseData(testValue, "Date", GenerateExpected(testValue)); //Operand type clash: DATE is incompatible with TIME
                yield return new TestCaseData(testValue, "DateTime", GenerateExpected(testValue));
                yield return new TestCaseData(testValue, "SmallDateTime", GenerateExpected(testValue));
                yield return new TestCaseData(testValue, "BigDateTime", GenerateExpected(testValue));
                yield return new TestCaseData(testValue, "Time", GenerateExpected(testValue));
            }
        }

        private static DateTime? GenerateExpected(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            var time = value.Value.TimeOfDay;
            return new DateTime(1900, 01, 01, time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }
    }
}
