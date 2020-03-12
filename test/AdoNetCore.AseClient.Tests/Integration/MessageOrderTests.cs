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
@"CREATE PROCEDURE [dbo].[sp_test_message_order] (@runSelect char = 'Y', @status varchar(10) output)
AS
BEGIN

PRINT 'Report Header'
PRINT 'Table 1 Header'
if @runSelect = 'Y'
begin
   select 'value1'
end
PRINT 'Empty Table 2 Header'
if @runSelect = 'Y'
begin
   select 'value2' where 1 = 2
end
PRINT 'Table 3 Header'
if @runSelect = 'Y'
begin
   select 'value3'
end
PRINT 'Report Trailer'

select @status = 'OK'

END";

        private readonly string _dropProc =
@"IF EXISTS(SELECT 1 FROM sysobjects WHERE name = 'sp_test_message_order')
BEGIN
    DROP PROCEDURE [dbo].[sp_test_message_order]
END";

        // ReSharper disable once InconsistentNaming
        private static readonly string[] _expectedResultsWithoutSelect = { "Report Header", "Table 1 Header", "Empty Table 2 Header", "Table 3 Header", "Report Trailer" };

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
                        command.Parameters.Add("@runSelect", withSelect);
                        command.Parameters.Add(new AseParameter("@status", AseDbType.VarChar) { Direction = ParameterDirection.Output });
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
                        Assert.AreEqual("OK", command.Parameters["@status"].Value.ToString());
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
                        command.Parameters.Add("@runSelect", 'N');
                        command.Parameters.Add(new AseParameter("@status", AseDbType.VarChar) { Direction = ParameterDirection.Output });
                        // ReSharper disable once UnusedVariable
                        using (var reader = command.ExecuteReader())
                        {
                            // Do not attempt to read results for this test
                            // server output should still be sent
                        }
                        Assert.AreEqual("OK", command.Parameters["@status"].Value.ToString());
                    }
                }
                finally
                {
                    connection.InfoMessage -= messageEventHandler;
                }
            }

            CollectionAssert.AreEqual(_expectedResultsWithoutSelect, results.ToArray());
        }

        [TestCaseSource(nameof(StoredProcTestCases))]
        public void ExecuteReader_WithMessagesEmbeddedInResultsAndUsingDataTable_RetainsServerOrder(char withSelect, string[] expectedResults)
        {
            var results = new List<string>();
            var output = new List<string>();

            var messageEventHandler = new AseInfoMessageEventHandler((sender, eventArgs) =>
            {
                foreach (AseError error in eventArgs.Errors)
                {
                    output.Add(error.Message);
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
                        command.Parameters.Add("@runSelect", withSelect);
                        command.Parameters.Add(new AseParameter("@status", AseDbType.VarChar) { Direction = ParameterDirection.Output });

                        using (var reader = command.ExecuteReader())
                        {
                            MergeOuput();

                            int resultCount = 0;
                            if (withSelect == 'Y')
                            {
                                // Cursor 0 exists and has rows
                                Assert.False(reader.IsClosed);
                                Assert.True(reader.HasRows);
                            }
                            else
                            {
                                Assert.True(reader.IsClosed);
                                Assert.False(reader.HasRows);
                            }
                            while (!reader.IsClosed)
                            {
                                MergeOuput();

                                var dataTable = new DataTable();
                                dataTable.Load(reader);
                                if (dataTable.Rows.Count > 0)
                                {
                                    results.Add(dataTable.Rows[0].ItemArray[0].ToString());
                                }

                                // Only add the next output after the previous results
                                // have been inserted
                                MergeOuput();

                                resultCount++;
                                switch(resultCount)
                                {
                                    // Cursor 1 exists and has no rows
                                    case 1:
                                        Assert.False(reader.IsClosed);
                                        Assert.False(reader.HasRows);
                                        break;
                                    // Cursor 2 exists and has rows
                                    case 2:
                                        Assert.False(reader.IsClosed);
                                        Assert.True(reader.HasRows);
                                        break;
                                    // Cursor 3 does not exist and so has no rows
                                    case 3:
                                        Assert.True(reader.IsClosed);
                                        Assert.False(reader.HasRows);
                                        break;
                                }
                            }

                            MergeOuput();
                        }
                        Assert.AreEqual("OK", command.Parameters["@status"].Value.ToString());
                    }
                }
                finally
                {
                    connection.InfoMessage -= messageEventHandler;
                }
            }

            CollectionAssert.AreEqual(expectedResults, results.ToArray());

            void MergeOuput()
            {
                results.AddRange(output);
                output.Clear();
            }
        }

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
                        command.Parameters.Add("@runSelect", withSelect);
                        command.Parameters.Add(new AseParameter("@status", AseDbType.VarChar) { Direction = ParameterDirection.Output });

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
                        Assert.AreEqual("OK", command.Parameters["@status"].Value.ToString());
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
            yield return new TestCaseData('Y', new[] { "Report Header", "Table 1 Header", "value1", "Empty Table 2 Header", "Table 3 Header", "value3", "Report Trailer" });
            yield return new TestCaseData('N', _expectedResultsWithoutSelect);
        }
    }
}
