using System.Data;
using System.Text.RegularExpressions;
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
        [TestCase("SELECT 1, 'What is going on ?' FROM myTable WHERE myColumn = ?", @"SELECT 1,'What is going on ?' FROM myTable WHERE myColumn = @p0")]
        public void TestReplacement(string input, string expected)
        {
            // Match a literal question mark if preceded by a comparison operator.
            // BUG - this matches the operator and the whitespace.
            var regex = new System.Text.RegularExpressions.Regex(
                @"(?:>=|<=|<>|!=|!>|!<|[^><!]=|[^<!]>|[^!]<)\s*([?])",
                RegexOptions.Compiled | RegexOptions.Multiline);

            int i = 0;

            MatchEvaluator evaluator = delegate(Match match)
            {
                return $"@p{i++}";
            };

            var actual = regex.Replace(input, evaluator);

            Assert.AreEqual(expected, actual);
        }
    }
}
