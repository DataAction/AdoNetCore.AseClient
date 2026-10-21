using System;
using System.Data;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    public partial class AseDataReaderTests<T> where T : IConnectionProvider
    {
        // CommandBehavior.CloseConnection: "When the command is executed, the associated Connection object is closed when the
        // associated DataReader object is closed."
        [Test]
        public void ExecuteReader_WithCloseConnection_ClosesTheConnectionWithTheReader()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select data_field from insert_records_tests";
                    var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                    }

                    Assert.AreEqual(ConnectionState.Open, connection.State);
                    reader.Dispose(); // closes the reader; DbDataReader.Close is not available on every target framework
                    Assert.AreEqual(ConnectionState.Closed, connection.State);
                }
            }
        }
    }
}
