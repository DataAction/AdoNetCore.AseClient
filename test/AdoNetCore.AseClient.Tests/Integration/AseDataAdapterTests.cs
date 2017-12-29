#if !NETCORE_OLD
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class AseDataAdapterTests
    {
        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Use SqlCommandBuilder.
            using (var connnection = new AseConnection(_connectionStrings["default"]))
            {
                connnection.Open();

                using (var command = connnection.CreateCommand())
                {
                    command.CommandText = 
@"IF OBJECT_ID('AseDataAdapterTests_Table1') IS NOT NULL 
BEGIN 
    DROP TABLE AseDataAdapterTests_Table1
END";
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE TABLE AseDataAdapterTests_Table1(ColumnId INT IDENTITY PRIMARY KEY, ColumnDescription VARCHAR(256) NOT NULL)";
                    command.ExecuteNonQuery();

                    for (var i = 0; i < 5; i++)
                    {
                        command.CommandText = $"INSERT INTO AseDataAdapterTests_Table1(ColumnDescription) VALUES('SomeText{i}')";
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // Use SqlCommandBuilder.
            using (var connnection = new AseConnection(_connectionStrings["default"]))
            {
                connnection.Open();

                using (var command = connnection.CreateCommand())
                {
                    command.CommandText = "DROP TABLE AseDataAdapterTests_Table1";
                    command.ExecuteNonQuery();
                }
            }
        }


        [Test]
        public void AseAdapter_WithAseCommandBuilder_HasInsertUpdateDeleteCommands()
        {
            // Use SqlCommandBuilder.
            using (var connnection = new AseConnection(_connectionStrings["default"]))
            {
                connnection.Open();

                using (var adapter = new AseDataAdapter("SELECT ColumnId, ColumnDescription FROM AseDataAdapterTests_Table1", connnection))
                {
                    using (var builder = new AseCommandBuilder(adapter))
                    {
                        // Check that the AseCommandBuilder is initialised.
                        var insertCommand = builder.GetInsertCommand();
                        Assert.IsNotNull(insertCommand);
                        StringAssert.Contains("INSERT", insertCommand.CommandText);

                        var updateCommand = builder.GetUpdateCommand();
                        Assert.IsNotNull(updateCommand);
                        StringAssert.Contains("UPDATE", updateCommand.CommandText);

                        var deleteCommand = builder.GetDeleteCommand();
                        Assert.IsNotNull(deleteCommand);
                        StringAssert.Contains("DELETE", deleteCommand.CommandText);

                        // Check that the AseDataAdapter is initialised.
                        Assert.IsNotNull(adapter.InsertCommand);
                        StringAssert.Contains("INSERT", adapter.InsertCommand.CommandText);

                        Assert.IsNotNull(adapter.UpdateCommand);
                        StringAssert.Contains("UPDATE", adapter.UpdateCommand.CommandText);

                        Assert.IsNotNull(adapter.DeleteCommand);
                        StringAssert.Contains("DELETE", adapter.DeleteCommand.CommandText);
                    }
                }
            }
        }
    }
}
#endif
