using System.Data;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class NamedParamsTests
    {
        [Test]
        public void ExecuteProcedure_WithNamedParametersFalse_ReturnsParameterValues()
        {
            using (var connection = new AseConnection(ConnectionStrings.NamedParametersOff))
            {
                connection.Open();

                Assert.IsFalse(connection.NamedParameters);

                using (var command = connection.CreateCommand())
                {
                    Assert.IsFalse(command.NamedParameters);

                    command.CommandType = CommandType.Text;
                    command.CommandText =
                        @"IF OBJECT_ID('EchoParameter') IS NOT NULL 
BEGIN 
    DROP PROCEDURE EchoParameter
END";
                    command.ExecuteNonQuery();

                    command.CommandType = CommandType.Text;
                    command.CommandText =
                        @"CREATE PROCEDURE [EchoParameter]
(
    @parameter1 VARCHAR(256),
    @parameter2 VARCHAR(256)
)
AS 
BEGIN 
    SELECT @parameter1 + ' ' + @parameter2 AS Value
END";
                    command.ExecuteNonQuery();


                    command.CommandType = CommandType.StoredProcedure;
                    command.AseParameters.Add(0, "General Kenobi?");
                    command.AseParameters.Add(1, "Hello there!");
                    command.CommandText = "EchoParameter";

                    var result = command.ExecuteScalar();

                    Assert.AreEqual("General Kenobi? Hello there!", result);
                }
            }
        }

        [Test]
        public void ExecuteProcedure_WithNamedParametersTrue_ReturnsParameterValue()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                Assert.IsTrue(connection.NamedParameters);

                using (var command = connection.CreateCommand())
                {
                    Assert.IsTrue(command.NamedParameters);

                    command.CommandType = CommandType.Text;
                    command.CommandText =
                        @"IF OBJECT_ID('EchoParameter') IS NOT NULL 
BEGIN 
    DROP PROCEDURE EchoParameter
END";
                    command.ExecuteNonQuery();

                    command.CommandType = CommandType.Text;
                    command.CommandText =
                        @"CREATE PROCEDURE [EchoParameter]
(
    @parameter1 VARCHAR(256),
    @parameter2 VARCHAR(256)
)
AS 
BEGIN 
    SELECT @parameter1 + ' ' + @parameter2 AS Value
END";
                    command.ExecuteNonQuery();


                    command.CommandType = CommandType.StoredProcedure;
                    command.AseParameters.Add("@parameter1", "General Kenobi?");
                    command.AseParameters.Add("@parameter2", "Hello there!");
                    command.CommandText = "EchoParameter";

                    var result = command.ExecuteScalar();

                    Assert.AreEqual("General Kenobi? Hello there!", result);
                }
            }
        }

        [Test]
        public void ExecuteQuery_WithNamedParametersFalse_ReturnsParameterValues()
        {
            using (var connection = new AseConnection(ConnectionStrings.NamedParametersOff))
            {
                connection.Open();

                Assert.IsFalse(connection.NamedParameters);

                using (var command = connection.CreateCommand())
                {
                    Assert.IsFalse(command.NamedParameters);
                    
                    command.CommandType = CommandType.Text;
                    command.AseParameters.Add(0, "General Kenobi?");
                    command.AseParameters.Add(1, "Hello there!");
                    command.CommandText = "SELECT ? + ' ' + ? AS Value";

                    var result = command.ExecuteScalar();

                    Assert.AreEqual("General Kenobi? Hello there!", result);
                }
            }
        }

        [Test]
        public void ExecuteQuery_WithNamedParametersFalse_ReturnsParameterValues2()
        {
            using (var connection = new AseConnection(ConnectionStrings.NamedParametersOff))
            {
                connection.Open();

                Assert.IsFalse(connection.NamedParameters);

                using (var command = connection.CreateCommand())
                {
                    Assert.IsFalse(command.NamedParameters);
                    
                    command.CommandType = CommandType.Text;
                    command.AseParameters.Add(new AseParameter { Value = "syscolumns" } );
                    command.CommandText = "SELECT TOP 1 name FROM sysobjects WHERE name = ?";

                    var result = command.ExecuteScalar();

                    Assert.AreEqual("syscolumns", result);
                }
            }
        }

        [Test]
        public void ExecuteQuery_WithNamedParametersTrue_ReturnsParameterValues2()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();

                Assert.IsTrue(connection.NamedParameters);

                using (var command = connection.CreateCommand())
                {
                    Assert.IsTrue(command.NamedParameters);
                    
                    command.CommandType = CommandType.Text;
                    command.AseParameters.Add("@objectName", "syscolumns");
                    command.CommandText = "SELECT TOP 1 name FROM sysobjects WHERE name = @objectName";

                    var result = command.ExecuteScalar();

                    Assert.AreEqual("syscolumns", result);
                }
            }
        }

        [TestCase("SELECT 1 FROM myTable WHERE myColumn = ?", "SELECT 1 FROM myTable WHERE myColumn = @p0")]
        [TestCase("SELECT 1 FROM myTable WHERE myColumn=?", "SELECT 1 FROM myTable WHERE myColumn=@p0")]
        [TestCase("SELECT 1, [some column ?] FROM myTable WHERE myColumn = ?", "SELECT 1, [some column ?] FROM myTable WHERE myColumn = @p0")]
        [TestCase(@"SELECT 1, ""What is going on ?"" FROM myTable WHERE myColumn = ?", @"SELECT 1, ""What is going on ?"" FROM myTable WHERE myColumn = @p0")]
        [TestCase("SELECT 1, 'What is going on ?' FROM myTable WHERE myColumn = ?", @"SELECT 1, 'What is going on ?' FROM myTable WHERE myColumn = @p0")]
        [TestCase("SELECT 1 FROM myTable WHERE myColumn1 = ? AND myColumn2 = ?", "SELECT 1 FROM myTable WHERE myColumn1 = @p0 AND myColumn2 = @p1")]
        [TestCase("SELECT 1 FROM myTable WHERE myColumn1 = ? AND myColumn2 = 'A question?' AND myColumn3 = ?", "SELECT 1 FROM myTable WHERE myColumn1 = @p0 AND myColumn2 = 'A question?' AND myColumn3 = @p1")]
        [TestCase("SELECT 1, 'What is going on ?', \"What is going on ?\", [What is going on ?] FROM myTable WHERE myColumn = ?", "SELECT 1, 'What is going on ?', \"What is going on ?\", [What is going on ?] FROM myTable WHERE myColumn = @p0")]
        [TestCase("SELECT 1, 'What is going on \"arggh\"', \"What is going on ?\" FROM myTable WHERE myColumn = ?", "SELECT 1, 'What is going on \"arggh\"', \"What is going on ?\" FROM myTable WHERE myColumn = @p0")]
        [TestCase("SELECT 1, 'What is going on \"arggh', \"What is going on ?\" FROM myTable WHERE myColumn = ?", "SELECT 1, 'What is going on \"arggh', \"What is going on ?\" FROM myTable WHERE myColumn = @p0")]
        [TestCase("SELECT 1, 'What is going on \"arggh?', \"What is going on ?\" FROM myTable WHERE myColumn = ?", "SELECT 1, 'What is going on \"arggh?', \"What is going on ?\" FROM myTable WHERE myColumn = @p0")]
        [TestCase("SELECT 1, 'What is going on \"arggh?\"', \"What is going on ?\" FROM myTable WHERE myColumn = ?", "SELECT 1, 'What is going on \"arggh?\"', \"What is going on ?\" FROM myTable WHERE myColumn = @p0")]
        [TestCase("EXEC myProc ?,?,?,?", "EXEC myProc @p0,@p1,@p2,@p3")]
        [TestCase("EXEC myProc ?, ?, ?, ?", "EXEC myProc @p0, @p1, @p2, @p3")]
        public void ToNamedParameters_WithQuestionMarkQuery_SubstitutesCorrectly(string input, string expected)
        {
            var actual = input.ToNamedParameters();

            Assert.AreEqual(expected, actual);
        }
    }
}
