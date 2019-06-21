using System;
using System.Collections.Generic;
using System.Data;
using AdoNetCore.AseClient.Internal;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("streaming")]
    public class StreamingTests
    {
        private static string CleanUp =
@"IF EXISTS(SELECT 1 FROM sysobjects WHERE name = 'stream-messages')
BEGIN
    DROP PROCEDURE [dbo].[stream-messages]
END";

        private static readonly string TestProcStreamMessages =
@"CREATE PROCEDURE [dbo].[stream-messages]
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
                connection.Execute(TestProcStreamMessages);
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

        [Test]
        public void Connection_InfoMessage_WithSetFlushMessageOn_ReturnsInRealTime()
        {
            const int numLoops = 5;
            var sleep = TimeSpan.FromSeconds(1);
            var messages = new List<Tuple<string, DateTime>>();

            using (var connection = GetConnection())
            {
                connection.Open();

                connection.InfoMessage += (sender, args) =>
                {
                    messages.Add(new Tuple<string, DateTime>(args.Message, DateTime.UtcNow)); // Capture the time when the message was received.
                };

                using (var command = connection.CreateCommand())
                {
                    // Tell the server to flush the messages.
                    command.CommandText = "SET FLUSHMESSAGE ON";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    var sleepTime = command.CreateParameter();
                    sleepTime.ParameterName = "@sleepTime";
                    sleepTime.Value = sleep.ToString();

                    var numberOfLoops = command.CreateParameter();
                    numberOfLoops.ParameterName = "@numberOfLoops";
                    numberOfLoops.Value = numLoops;

                    command.CommandText = "[dbo].[stream-messages]";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(sleepTime);
                    command.Parameters.Add(numberOfLoops);
                    
                    command.ExecuteNonQuery();

                    Assert.AreEqual(numLoops, messages.Count, $"Expected there to be {numLoops} messages returned from the stored procedure.");


                    for (var i = 0; i < messages.Count - 1; i++)
                    {
                        var current = messages[i];
                        var next = messages[i + 1];

                        Assert.AreEqual("General Kenobi", current.Item1, "Unexpected message");

                        var expectedTime = current.Item2 + sleep + TimeSpan.FromSeconds(0.5);

                        Assert.Less(next.Item2, expectedTime); // We expect that the processing time occurs within 1s of the sleep time.
                    }
                }
            }
        }
    }
}
