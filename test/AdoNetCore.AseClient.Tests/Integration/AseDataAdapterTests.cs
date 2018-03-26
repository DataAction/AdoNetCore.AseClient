#if !NETCORE_OLD
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("extra")]
    public class AseDataAdapterTests
    {
        [SetUp]
        public void SetUp()
        {
            // Use SqlCommandBuilder.
            using (var connnection = new AseConnection(ConnectionStrings.Default))
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

                    command.CommandText = "CREATE TABLE AseDataAdapterTests_Table1(ColumnId INT IDENTITY PRIMARY KEY, ColumnDescription VARCHAR(256) NOT NULL, ColumnNullable VARCHAR(256) NULL)";
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

        [TearDown]
        public void TearDown()
        {
            // Use SqlCommandBuilder.
            using (var connnection = new AseConnection(ConnectionStrings.Default))
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
            using (var connnection = new AseConnection(ConnectionStrings.Default))
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
        
        [Test]
        public void Test()
        {

        }

        [TestCaseSource(nameof(DeriveParametersTestCases))]
        public void AseCommandBuilder_DeriveParameters_SetsParametersOnCommand(string procedureName)
        {
            using (var connnection = new AseConnection(ConnectionStrings.Default))
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

        /// <summary>
        /// Dynamic test case depending on the name of the database in use.
        /// </summary>
        /// <returns>Test cases with different variants of the same proc name.</returns>
        private static IEnumerable<TestCaseData> DeriveParametersTestCases()
        {
            var connectionString = ConnectionStrings.Default;
            var match = Regex.Match(connectionString, @"(((Database)|(Db)|(Initial Catalog))\s*=\s*(?<database>[^;]+))", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            string database = null;
            if (match.Success)
            {
                database = match.Groups["database"].Value;
            }

            if (string.IsNullOrWhiteSpace(database))
            {
                throw new AssertionException("The 'default' connection string does not specify a 'Database' property.");
            }

            yield return new TestCaseData("AseDataAdapterTests_Proc1");
            yield return new TestCaseData("dbo.AseDataAdapterTests_Proc1");
            yield return new TestCaseData($"{database}.dbo.AseDataAdapterTests_Proc1");
            yield return new TestCaseData($"{database}..AseDataAdapterTests_Proc1");
        }

        [Test]
        public void AseAdapter_WithAseCommandBuilder_CanInsertUpdateAndDelete()
        {
            using (var connnection = new AseConnection(ConnectionStrings.Default))
            {
                connnection.Open();

                using (var adapter = new AseDataAdapter("SELECT ColumnId, ColumnDescription, ColumnNullable, COALESCE(ColumnNullable, 'Foo') AS ColumnCalculated FROM AseDataAdapterTests_Table1", connnection))
                {
                    using (new AseCommandBuilder(adapter))
                    {
                        var original = new DataTable("AseDataAdapterTests_Table1");
                        adapter.FillSchema(original, SchemaType.Mapped);
                        adapter.Fill(original);

                        Assert.AreEqual(5, original.Rows.Count); // SELECT

                        var updateRow = original.Rows.Find(1);
                        Assert.IsNotNull(updateRow, "Did not find a row in AseDataAdapterTests_Table1 for update with ColumnId=1");
                        updateRow["ColumnDescription"] = "an updated value"; // UPDATE

                        var deleteRow = original.Rows.Find(3);
                        Assert.IsNotNull(deleteRow, "Did not find a row in AseDataAdapterTests_Table1 for delete with ColumnId=3");
                        deleteRow.Delete(); // DELETE

                        original.Rows.Add(-1, "an inserted value"); // INSERT

                        // Commit the changes to the database.
                        adapter.Update(original);
                        original.AcceptChanges();

                        var fresh = new DataTable("AseDataAdapterTests_Table1");
                        adapter.FillSchema(fresh, SchemaType.Mapped);
                        adapter.Fill(fresh);

                        Assert.AreEqual(5, fresh.Rows.Count); // SELECT

                        updateRow = fresh.Rows.Find(1);
                        Assert.IsNotNull(updateRow, "Did not find a row in AseDataAdapterTests_Table1 for update with ColumnId=1");
                        Assert.AreEqual(updateRow["ColumnDescription"], "an updated value");

                        deleteRow = fresh.Rows.Find(3);
                        Assert.IsNull(deleteRow);

                        var insertRow = fresh.Rows.Find(6); // Next identity value.
                        Assert.IsNotNull(insertRow);
                    }
                }
            }
        }
    }
}
#endif
