#if !NETCORE_OLD
using System.Collections.Generic;
using System.Data;
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

                    command.CommandText =
                        @"IF OBJECT_ID('AseDataAdapterTests_Proc1') IS NOT NULL 
BEGIN 
    DROP PROCEDURE AseDataAdapterTests_Proc1
END";
                    command.ExecuteNonQuery();

                    command.CommandText =
@"CREATE PROCEDURE AseDataAdapterTests_Proc1
(
    @p1 INT,
    @p2 INT = NULL,
    @p3 SMALLINT,
    @p4 SMALLINT = NULL,
    @p5 BIGINT,
    @p6 BIGINT = NULL,
    @p7 TINYINT,
    @p8 TINYINT = NULL,

    @p9 UNSIGNED INT,
    @p10 UNSIGNED INT = NULL,
    @p11 UNSIGNED SMALLINT,
    @p12 UNSIGNED SMALLINT = NULL,
    @p13 UNSIGNED BIGINT,
    @p14 UNSIGNED BIGINT = NULL,
    @p15 UNSIGNED TINYINT,
    @p16 UNSIGNED TINYINT = NULL,

    @p17 REAL,
    @p18 REAL = NULL,
    @p19 DOUBLE PRECISION,
    @p20 DOUBLE PRECISION = NULL,
    @p21 NUMERIC(18,6),
    @p22 NUMERIC(18,6) = NULL,

    @p23 MONEY,
    @p24 MONEY = NULL,
    @p25 SMALLMONEY,
    @p26 SMALLMONEY = NULL,

    @p27 BIT,
    @p28 BINARY(16),
    @p29 BINARY(16) = NULL,
    @p30 VARBINARY(16),
    @p31 VARBINARY(16) = NULL,
    @p32 IMAGE,
    @p33 IMAGE = NULL,

    @p34 VARCHAR(512),
    @p35 VARCHAR(512) = NULL,
    @p36 CHAR(512),
    @p37 CHAR(512) = NULL,
    @p38 UNIVARCHAR(512),
    @p39 UNIVARCHAR(512) = NULL,
    @p40 UNICHAR(512),
    @p41 UNICHAR(512) = NULL,
    @p42 TEXT,
    @p43 TEXT = NULL,
    @p44 UNITEXT,
    @p45 UNITEXT = NULL,

    --@p46 BIGDATETIME,
    --@p47 BIGDATETIME = NULL,
    @p48 DATETIME,
    @p49 DATETIME = NULL,
    @p50 SMALLDATETIME,
    @p51 SMALLDATETIME = NULL,
    @p52 DATE,
    @p53 DATE = NULL,

    --@p54 BIGTIME,
    --@p55 BIGTIME = NULL,
    @p56 TIME,
    @p57 TIME = NULL,

    @o1 INT OUTPUT
)
AS 
BEGIN 
    SELECT 
        1 AS id, 
        'something' as description 
    RETURN 0
END";
                    command.ExecuteNonQuery();
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
                    command.CommandText =
                        @"IF OBJECT_ID('AseDataAdapterTests_Table1') IS NOT NULL 
BEGIN 
    DROP TABLE AseDataAdapterTests_Table1
END";
                    command.ExecuteNonQuery();

                    command.CommandText =
                        @"IF OBJECT_ID('AseDataAdapterTests_Proc1') IS NOT NULL 
BEGIN 
    DROP PROCEDURE AseDataAdapterTests_Proc1
END";
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

        [TestCase("AseDataAdapterTests_Proc1")]
        [TestCase("dbo.AseDataAdapterTests_Proc1")]
        [TestCase("pubs2.dbo.AseDataAdapterTests_Proc1")]
        [TestCase("pubs2..AseDataAdapterTests_Proc1")]
        public void AseCommandBuilder_DeriveParameters_SetsParametersOnCommand(string procedureName)
        {
            // Use SqlCommandBuilder.
            using (var connnection = new AseConnection(_connectionStrings["default"]))
            {
                connnection.Open();

                using (var command = connnection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = procedureName;
                    AseCommandBuilder.DeriveParameters(command);
                    
                    Assert.AreEqual(55, command.Parameters.Count);
                    Assert.AreEqual(ParameterDirection.ReturnValue, command.Parameters["@RETURN_VALUE"].Direction);
                    Assert.AreEqual(ParameterDirection.Output, command.Parameters["@o1"].Direction);
                }
            }
        }
    }
}
#endif
