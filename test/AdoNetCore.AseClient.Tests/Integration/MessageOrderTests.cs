using System.Collections.Generic;
using System.Data;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{

    [TestFixture]
    public class MessageOrderTests
    {
        //echo an int
        private readonly string _createProc =
@"CREATE PROCEDURE [dbo].[sp_test_message_order]
AS
BEGIN

PRINT 'Report Header'
PRINT 'Table 1 Header'
select 'value1'
PRINT 'Table 2 Header'
select 'value2'
PRINT 'Report Trailer'

END";

        private readonly string _dropProc =
@"IF EXISTS(SELECT 1 FROM sysobjects WHERE name = 'sp_test_message_order')
BEGIN
    DROP PROCEDURE [dbo].[sp_test_message_order]
END";

        [SetUp]
        public void SetUp()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Execute(_dropProc);
                connection.Execute(_createProc);
            }
        }

        [TearDown]
        public void Teardown()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Execute(_dropProc);
            }
        }

        [Test]
        public void ExecuteReader_WithMessagesEmbeddedInResults_RetainsServerOrder()
        {
            var results = new List<string>();

            var messageEventHandler = new AseInfoMessageEventHandler((sender, eventArgs) =>
            {
                foreach (AseError error in eventArgs.Errors)
                {
                    results.Add(error.Message);
                }
            });

            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();

                try
                {
                    connection.InfoMessage += messageEventHandler;

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "[dbo].[sp_test_message_order]";
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = command.ExecuteReader())
                        {
                            do
                            {
                                while (reader.Read())
                                {
                                    results.Add(reader.GetString(0));
                                }
                            } while (reader.NextResult());
                        }
                    }
                }
                finally
                {
                    connection.InfoMessage -= messageEventHandler;
                }
            }

            var expected = new[] { "Report Header", "Table 1 Header", "value1", "Table 2 Header", "value2", "Report Trailer"};

            CollectionAssert.AreEqual(expected, results.ToArray());
        }

        [Test]
        public void ExecuteReader_WithMessagesEmbeddedInResultsAndFlushMessageOn_RetainsServerOrder()
        {
            var results = new List<string>();

            var messageEventHandler = new AseInfoMessageEventHandler((sender, eventArgs) =>
            {
                foreach (AseError error in eventArgs.Errors)
                {
                    results.Add(error.Message);
                }
            });

            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();

                connection.Execute("SET FLUSHMESSAGE ON");

                try
                {
                    connection.InfoMessage += messageEventHandler;

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "[dbo].[sp_test_message_order]";
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = command.ExecuteReader())
                        {
                            do
                            {
                                while (reader.Read())
                                {
                                    results.Add(reader.GetString(0));
                                }
                            } while (reader.NextResult());
                        }
                    }
                }
                finally
                {
                    connection.InfoMessage -= messageEventHandler;
                }
            }

            var expected = new[] { "Report Header", "Table 1 Header", "value1", "Table 2 Header", "value2", "Report Trailer"};

            CollectionAssert.AreEqual(expected, results.ToArray());
        }
    }
}
