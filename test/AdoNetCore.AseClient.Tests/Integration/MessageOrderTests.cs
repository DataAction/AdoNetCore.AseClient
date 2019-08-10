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
@"CREATE PROCEDURE [dbo].[sp_test_message_order] (@runSelect char = 'Y')
AS
BEGIN

PRINT 'Report Header'
PRINT 'Table 1 Header'
if @runSelect = 'Y'
begin
   select 'value1'
end
PRINT 'Table 2 Header'
if @runSelect = 'Y'
begin
   select 'value2'
end
PRINT 'Report Trailer'

END";

        private readonly string _dropProc =
@"IF EXISTS(SELECT 1 FROM sysobjects WHERE name = 'sp_test_message_order')
BEGIN
    DROP PROCEDURE [dbo].[sp_test_message_order]
END";

        // ReSharper disable once InconsistentNaming
        private static readonly string[] _expectedResultsWithoutSelect = { "Report Header", "Table 1 Header", "Table 2 Header", "Report Trailer" };

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

        [TestCaseSource(nameof(StoredProcTestCases))]
        public void ExecuteReader_WithMessagesEmbeddedInResults_RetainsServerOrder(char withSelect, string[] expectedResults)
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
                        command.AseParameters.Add("@runSelect", withSelect);
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

            CollectionAssert.AreEqual(expectedResults, results.ToArray());
        }

        [Test]
        public void ExecuteReader_WithoutSelects_RetainsServerOrder()
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
                        command.AseParameters.Add("@runSelect", 'N');
                        // ReSharper disable once UnusedVariable
                        using (var reader = command.ExecuteReader())
                        {
                            // Do not attempt to read results for this test
                            // server output should still be sent
                        }
                    }
                }
                finally
                {
                    connection.InfoMessage -= messageEventHandler;
                }
            }

            CollectionAssert.AreEqual(_expectedResultsWithoutSelect, results.ToArray());
        }

#if (NETFRAMEWORK || (NETCOREAPP && !NETCOREAPP1_0 && !NETCOREAPP1_1))
        [TestCaseSource(nameof(StoredProcTestCases))]
        public void ExecuteReader_WithMessagesEmbeddedInResultsAndUsingDataTable_RetainsServerOrder(char withSelect, string[] expectedResults)
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
                            int resultCount = 0;
                            Assert.True(reader.HasRows);
                            while (reader.HasRows)
                            {
                                DataTable dataTable = new DataTable();
                                dataTable.Load(reader);
                                results.Add(dataTable.Rows[0].ItemArray[0].ToString());
                                resultCount++;
                                if (resultCount < 2)
                                {
                                    Assert.True(reader.HasRows);
                                }
                                else
                                {
                                    Assert.False(reader.HasRows);
                                }

                            }
                        }
                    }
                }
                finally
                {
                    connection.InfoMessage -= messageEventHandler;
                }
            }

            CollectionAssert.AreEqual(expectedResults, results.ToArray());
        }
#endif

        [TestCaseSource(nameof(StoredProcTestCases))]
        public void ExecuteReader_WithMessagesEmbeddedInResultsAndFlushMessageOn_RetainsServerOrder(char withSelect, string[] expectedResults)
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
                        command.AseParameters.Add("@runSelect", withSelect);

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

            CollectionAssert.AreEqual(expectedResults, results.ToArray());
        }


        private static IEnumerable<TestCaseData> StoredProcTestCases()
        {
            yield return new TestCaseData('Y', new[] { "Report Header", "Table 1 Header", "value1", "Table 2 Header", "value2", "Report Trailer" });
            yield return new TestCaseData('N', _expectedResultsWithoutSelect);
        }
    }
}
