using System;
using System.Collections.Generic;
using System.Data;
using AdoNetCore.AseClient.Internal;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("extra")]
    public class StreamingTests
    {
        private static string CleanUp =
@"IF EXISTS(SELECT 1 FROM sysobjects WHERE name = 'stream-result-sets')
BEGIN
    DROP PROCEDURE [dbo].[stream-result-sets]
END";

        private static string TestProc =
$@"CREATE PROCEDURE [dbo].[stream-result-sets]
(
    @sleepTime CHAR(8),
    @numberOfLoops INT
)
AS
BEGIN
    DECLARE @index INT
    SET @index = 0

    WHILE (@index < @numberOfLoops)
    BEGIN
        SELECT @index AS [Index], GETUTCDATE() AS Now, '{new string('x', 1024 * 8)}' AS LotsOfText
        PRINT 'General Kenobi'

        SET @index = @index + 1
        WAITFOR DELAY @sleepTime
    END
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
                connection.Execute(TestProc);
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

        // TODO - a failing test that shows the result sets cannot be streamed.
        // TODO - use this to implement an IEnumerable socket, an IEnumerable token reader, and make data reader work accordingly.
        [Test]
        public void DelayedResultSetTests()
        {
            const int NumLoops = 5;
            TimeSpan sleep = TimeSpan.FromSeconds(10);

            using (var connection = GetConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    var sleepTime = command.CreateParameter();
                    sleepTime.ParameterName = "@sleepTime";
                    sleepTime.Value = sleep.ToString();

                    var numberOfLoops = command.CreateParameter();
                    numberOfLoops.ParameterName = "@numberOfLoops";
                    numberOfLoops.Value = NumLoops;

                    command.CommandText = "[dbo].[stream-result-sets]";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(sleepTime);
                    command.Parameters.Add(numberOfLoops);

                    var results = new List<Tuple<DateTime, DateTime>>();
                    using (var reader = command.ExecuteReader())
                    {
                        do
                        {
                            while (reader.Read())
                            {
                                // Select the time that the server created the record, and the time that we are processing it.
                                results.Add(new Tuple<DateTime, DateTime>(reader.GetDateTime(1), DateTime.UtcNow));
                            }
                        } while (reader.NextResult());
                    }

                    foreach (var result in results)
                    {
                        var selectedTime = result.Item1;
                        var processedTime = result.Item2;

                        var expectedTime = selectedTime + sleep + TimeSpan.FromSeconds(0.5);
                        
                        Assert.Less(processedTime, expectedTime); // We expect that the processing time occurs within 1s of the sleep time.
                    }
                }
            }
        }
    }
}
