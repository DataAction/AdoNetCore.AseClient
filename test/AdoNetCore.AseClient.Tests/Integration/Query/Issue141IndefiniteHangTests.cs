using System;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Query
{
    [TestFixture]
    public class Issue141IndefiniteHangTests
    {
        private AseConnection GetConnection()
        {
            return new AseConnection(ConnectionStrings.Pooled);
        }

        private readonly string _createTestTable = @"create table dbo.indefinite_hang_test (v1 int, v2 datetime)";
        private readonly string _dropTestTable = @"drop table dbo.indefinite_hang_test";

        private readonly string _testQuery = @"SELECT v1 AS col1, v2 As col2
FROM indefinite_hang_test
WHERE v2 between @StartDate and @EndDate";

        [SetUp]
        public void SetUp()
        {
            using (var connection = GetConnection())
            {
                connection.Execute(_createTestTable);
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var connection = GetConnection())
            {
                connection.Execute(_dropTestTable);
            }
        }

        [Test]
        public async Task SelectDateTime_Between_ShouldNotHang()
        {
            var date = DateTime.Now;
            using (var connection = GetConnection())
            {
                connection.Open();
                using (AseCommand command = connection.CreateCommand())
                {
                    command.CommandText = _testQuery;
                    AseParameter startDateParam = command.Parameters.Add("@StartDate", AseDbType.DateTime);
                    startDateParam.Value = date.Date;
                    AseParameter endDateParameter = command.Parameters.Add("@EndDate", AseDbType.DateTime);
                    endDateParameter.Value = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
                    //This is where we hang forever
                    using (DbDataReader r = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await r.ReadAsync().ConfigureAwait(false))
                        {
                            //Do stuff here
                        }
                    }
                }
            }
        }
    }
}
