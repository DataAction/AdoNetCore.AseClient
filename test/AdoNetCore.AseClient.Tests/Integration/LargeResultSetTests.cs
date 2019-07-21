using System.Data;
using AdoNetCore.AseClient.Internal;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("basic")]
    public class LargeResultSetTests
    {
        private const string CleanUp =
@"IF EXISTS(SELECT 1 FROM sysobjects WHERE name = 'large-results')
BEGIN
    DROP PROCEDURE [dbo].[large-results]
END";

        private static readonly string TestProcLargeResults =
@"CREATE PROCEDURE [dbo].[large-results]
(
    @numberOfRecords INT
)
AS
BEGIN
    DECLARE @index INT
    SET @index = 0

    DECLARE @text CHAR(4000)
    SET @text = REPLICATE('X', 4000)

    CREATE TABLE #thousand (Id INT, Description0 TEXT)

    WHILE (@index < 1000)
    BEGIN
        INSERT INTO #thousand(Id, Description0) VALUES(@index, @text)

        SET @index = @index + 1
    END

    IF(@numberOfRecords = 1000)
    BEGIN
        SELECT Id, Description0 FROM #thousand
    END
    IF(@numberOfRecords = 10000)
    BEGIN
        SELECT TOP 10000 t1.Id, t1.Description0 FROM #thousand t1, #thousand as t2
    END
    IF(@numberOfRecords = 100000)
    BEGIN
        SELECT TOP 100000 t1.Id, t1.Description0 FROM #thousand t1, #thousand as t2
    END
    IF(@numberOfRecords = 500000)
    BEGIN
        SELECT TOP 500000 t1.Id, t1.Description0 FROM #thousand t1, #thousand as t2
    END

    DROP TABLE #thousand
END
";
        private AseConnection GetConnection()
        {
            Logger.Enable();
            return new AseConnection(ConnectionStrings.Pooled);
        }

        [SetUp]
        public void Setup()
        {
            using (var connection = GetConnection())
            {
                connection.Execute(CleanUp);
                connection.Execute(TestProcLargeResults);
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var connection = GetConnection())
            {
                connection.Execute(CleanUp);
            }
        }

        [TestCase(1000,     TestName = "4MB data")]
        [TestCase(10000,   TestName = "40MB data")]
        [TestCase(100000, TestName = "400MB data", Explicit = true)]
        [TestCase(500000,   TestName = "2GB data", Explicit = true)]
        public void ExecuteReader_WithLargeResultSet_Success(int numRecords)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    var numberOfLoops = command.CreateParameter();
                    numberOfLoops.ParameterName = "@numberOfRecords";
                    numberOfLoops.Value = numRecords;

                    command.CommandText = "[dbo].[large-results]";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(numberOfLoops);

                    var count = 0;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            count++;
                        }
                    }

                    Assert.AreEqual(numRecords, count, $"Expected there to be {numRecords} records returned from the stored procedure.");
                }
            }
        }
    }
}
