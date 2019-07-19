using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
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

        [TestCase("SELECT ? + ' ' + ? AS Value", "SELECT @p0 + ' ' + @p1 AS Value")]
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
        [TestCase("select 'abcdef', ?, 'ghijki', ? from foo", "select 'abcdef', @p0, 'ghijki', @p1 from foo")]
        [TestCase("select 'abc?def', ?, 'ghi?jki', ? from foo", "select 'abc?def', @p0, 'ghi?jki', @p1 from foo")]
        [TestCase("select 'abc\"def', ?, 'ghi\"jki', ? from foo", "select 'abc\"def', @p0, 'ghi\"jki', @p1 from foo")]
        [TestCase("select 'abc''def', ?, 'ghi''jki', ? from foo", "select 'abc''def', @p0, 'ghi''jki', @p1 from foo")]
        [TestCase("select 'abc[def', ?, 'ghi]jki', ? from foo", "select 'abc[def', @p0, 'ghi]jki', @p1 from foo")]
        [TestCase("select 'abc--def', ?, 'ghi--jki', ? from foo", "select 'abc--def', @p0, 'ghi--jki', @p1 from foo")]
        [TestCase("select 'abc//def', ?, 'ghi//jki', ? from foo", "select 'abc//def', @p0, 'ghi//jki', @p1 from foo")]
        [TestCase("select 'abc/*def', ?, 'ghi*/jki', ? from foo", "select 'abc/*def', @p0, 'ghi*/jki', @p1 from foo")]
        [TestCase("select \"abcdef\", ?, \"ghijki\", ? from foo", "select \"abcdef\", @p0, \"ghijki\", @p1 from foo")]
        [TestCase("select \"abc?def\", ?, \"ghi?jki\", ? from foo", "select \"abc?def\", @p0, \"ghi?jki\", @p1 from foo")]
        [TestCase("select \"abc[def\", ?, \"ghi]jki\", ? from foo", "select \"abc[def\", @p0, \"ghi]jki\", @p1 from foo")]
        [TestCase("select \"abc\"\"def\", ?, \"ghi\"\"jki\", ? from foo", "select \"abc\"\"def\", @p0, \"ghi\"\"jki\", @p1 from foo")]
        [TestCase("select \"abc'def\", ?, \"ghi'jki\", ? from foo", "select \"abc'def\", @p0, \"ghi'jki\", @p1 from foo")]
        [TestCase("select \"abc--def\", ?, \"ghi--jki\", ? from foo", "select \"abc--def\", @p0, \"ghi--jki\", @p1 from foo")]
        [TestCase("select \"abc//def\", ?, \"ghi//jki\", ? from foo", "select \"abc//def\", @p0, \"ghi//jki\", @p1 from foo")]
        [TestCase("select \"abc/*def\", ?, \"ghi*/jki\", ? from foo", "select \"abc/*def\", @p0, \"ghi*/jki\", @p1 from foo")]
        [TestCase("select [abcdef], ?, [ghijki], ? from foo", "select [abcdef], @p0, [ghijki], @p1 from foo")]
        [TestCase("select [abc?def], ?, [ghi?jki], ? from foo", "select [abc?def], @p0, [ghi?jki], @p1 from foo")]
        [TestCase("select [abc[def], ?, [ghi]]jki], ? from foo", "select [abc[def], @p0, [ghi]]jki], @p1 from foo")]
        [TestCase("select [abc\"def], ?, [ghi\"jki], ? from foo", "select [abc\"def], @p0, [ghi\"jki], @p1 from foo")]
        [TestCase("select [abc'def], ?, [ghi'jki], ? from foo", "select [abc'def], @p0, [ghi'jki], @p1 from foo")]
        [TestCase("select [abc--def], ?, [ghi--jki], ? from foo", "select [abc--def], @p0, [ghi--jki], @p1 from foo")]
        [TestCase("select [abc//def], ?, [ghi//jki], ? from foo", "select [abc//def], @p0, [ghi//jki], @p1 from foo")]
        [TestCase("select [abc/*def], ?, [ghi*/jki], ? from foo", "select [abc/*def], @p0, [ghi*/jki], @p1 from foo")]
        [TestCase("select /*abcdef*/, ?, /*ghijki*/, ? from foo", "select /*abcdef*/, @p0, /*ghijki*/, @p1 from foo")]
        [TestCase("select /*abc?def*/, ?, /*ghi?jki*/, ? from foo", "select /*abc?def*/, @p0, /*ghi?jki*/, @p1 from foo")]
        [TestCase("select /*abc\"def*/, ?, /*ghi\"jki*/, ? from foo", "select /*abc\"def*/, @p0, /*ghi\"jki*/, @p1 from foo")]
        [TestCase("select /*abc'def*/, ?, /*ghi'jki*/, ? from foo", "select /*abc'def*/, @p0, /*ghi'jki*/, @p1 from foo")]
        [TestCase("select /*abc[def*/, ?, /*ghi]jki*/, ? from foo", "select /*abc[def*/, @p0, /*ghi]jki*/, @p1 from foo")]
        [TestCase("select /*abc--def*/, ?, /*ghi--jki*/, ? from foo", "select /*abc--def*/, @p0, /*ghi--jki*/, @p1 from foo")]
        [TestCase("select /*abc//def*/, ?, /*ghi//jki*/, ? from foo", "select /*abc//def*/, @p0, /*ghi//jki*/, @p1 from foo")]
        [TestCase("select /*abc/*def*/, ?, /*ghi*/jki*/, ? from foo", "select /*abc/*def*/, @p0, /*ghi*/jki*/, @p1 from foo")]
        [TestCase("select ?, -- abcdef, ?, ghijkl, ? from foo", "select @p0, -- abcdef, ?, ghijkl, ? from foo")]
        [TestCase("select ?, -- abcdef, ?,\n ghijkl, ? from foo", "select @p0, -- abcdef, ?,\n ghijkl, @p1 from foo")]
        [TestCase("select ?, // abcdef, ?, ghijkl, ? from foo", "select @p0, // abcdef, ?, ghijkl, ? from foo")]
        [TestCase("select ?, // abcdef, ?,\n ghijkl, ? from foo", "select @p0, // abcdef, ?,\n ghijkl, @p1 from foo")]
        [TestCase("select foo.*, bar.x/bar.y [xy?! ratio], bar.a-bar.b \"ab?! difference\", -- these names are awful, what are they for?\n"
            + " coalesce(bar.g, '/*??*/') \"abc \"\" def\" /* using block comments with ? as a null placeholder value for legacy code handling */, "
            + " ?/?+bar-? [[var1?]]], // why are all these names so bad?  why do they all contains ?\n"
            + " ? / ? * bar.z - ? \"\"\"var2?\"\"\"--,\n"
            + " /* // this seems unused? commenting out for testing \n ? / ? * bar.y - ? \"\"\"var3?\"\"\" */\n"
            + " from [table1] [foo]\n"
            + " left join \"table2\" \"bar\" on foo.k == bar.k\n"
            + " where bar.l == ? and bar.e == 'no value'",
            "select foo.*, bar.x/bar.y [xy?! ratio], bar.a-bar.b \"ab?! difference\", -- these names are awful, what are they for?\n"
            + " coalesce(bar.g, '/*??*/') \"abc \"\" def\" /* using block comments with ? as a null placeholder value for legacy code handling */, "
            + " @p0/@p1+bar-@p2 [[var1?]]], // why are all these names so bad?  why do they all contains ?\n"
            + " @p3 / @p4 * bar.z - @p5 \"\"\"var2?\"\"\"--,\n"
            + " /* // this seems unused? commenting out for testing \n ? / ? * bar.y - ? \"\"\"var3?\"\"\" */\n"
            + " from [table1] [foo]\n"
            + " left join \"table2\" \"bar\" on foo.k == bar.k\n"
            + " where bar.l == @p6 and bar.e == 'no value'")]
        // The following aren't really valid sql, but are useful for testing that the parameteriser is well-behaved in weird cases
        [TestCase("", "")]
        [TestCase("abcdef", "abcdef")]
        // TODO: the way parameters get accidentally merged looks suspicious, possibly append space to the end of them to prevent this type of corruption?
        // In practice this shouldn't happen, as parameters should always be followed by whitespace or symbols
        // eg select ? + foo.y, ?/foo.z, coalesce(foo.x, ?) from table1 foo where foo.a=?
        [TestCase("ab?cdef", "ab@p0cdef")]
        [TestCase("?abcdef", "@p0abcdef")]
        [TestCase("abcdef?", "abcdef@p0")]
        [TestCase("?abcdef?", "@p0abcdef@p1")]
        [TestCase("ab?cd?ef", "ab@p0cd@p1ef")]
        [TestCase("ab?c?d?ef", "ab@p0c@p1d@p2ef")]
        [TestCase("ab''cdef", "ab''cdef")]
        [TestCase("ab''''cdef", "ab''''cdef")]
        [TestCase("''abcdef", "''abcdef")]
        [TestCase("abcdef''", "abcdef''")]
        [TestCase("'abcdef'", "'abcdef'")]
        [TestCase("'abc?def'", "'abc?def'")]
        [TestCase("ab'cd'ef", "ab'cd'ef")]
        [TestCase("ab'c''d'ef", "ab'c''d'ef")]
        [TestCase("ab\"\"cdef", "ab\"\"cdef")]
        [TestCase("ab\"\"\"\"cdef", "ab\"\"\"\"cdef")]
        [TestCase("\"\"abcdef", "\"\"abcdef")]
        [TestCase("abcdef\"\"", "abcdef\"\"")]
        [TestCase("\"abcdef\"", "\"abcdef\"")]
        [TestCase("\"abc?def\"", "\"abc?def\"")]
        [TestCase("ab\"cd\"ef", "ab\"cd\"ef")]
        [TestCase("ab\"c\"\"d\"ef", "ab\"c\"\"d\"ef")]
        [TestCase("ab[]cdef", "ab[]cdef")]
        [TestCase("ab[]]]cdef", "ab[]]]cdef")]
        [TestCase("[]abcdef", "[]abcdef")]
        [TestCase("abcdef[]", "abcdef[]")]
        [TestCase("[abcdef]", "[abcdef]")]
        [TestCase("[abc?def]", "[abc?def]")]
        [TestCase("ab[cd]ef", "ab[cd]ef")]
        [TestCase("ab[c]]d]ef", "ab[c]]d]ef")]
        [TestCase("ab[c]]?d]ef", "ab[c]]?d]ef")]
        [TestCase("abc/def", "abc/def")]
        [TestCase("abc-def", "abc-def")]
        [TestCase("--abcdef", "--abcdef")]
        [TestCase("--abc?def", "--abc?def")]
        [TestCase("-/-abcdef", "-/-abcdef")]
        [TestCase("-/-abc?def", "-/-abc@p0def")]
        [TestCase("abc--def", "abc--def")]
        [TestCase("abc-/-def", "abc-/-def")]
        [TestCase("--abcdef\n", "--abcdef\n")]
        [TestCase("abc--def\n", "abc--def\n")]
        [TestCase("/*abcdef*/", "/*abcdef*/")]
        [TestCase("/*abc?def*/", "/*abc?def*/")]
        [TestCase("/-*abcdef*/", "/-*abcdef*/")]
        [TestCase("/-*abc?def*/", "/-*abc@p0def*/")]
        [TestCase("/*abc//def*/ghi", "/*abc//def*/ghi")]
        [TestCase("abc/*def\nghi*//jkl", "abc/*def\nghi*//jkl")]
        [TestCase("abc/*def\nghi*///jkl", "abc/*def\nghi*///jkl")]
        public void ToNamedParameters_WithQuestionMarkQuery_SubstitutesCorrectly(string input, string expected)
        {
            var actual = input.ToNamedParameters();

            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(SplitQuestionMarksInvalidSqlTestInput))]
        public void ToNamedParameters_WithQuestionMarkQuery_InvalidSql_Fails(string input)
        {
            Assert.Throws<ArgumentException>(() => input.ToNamedParameters());
        }

        public static IEnumerable<object[]> SplitQuestionMarksInvalidSqlTestInput =>
            new List<object[]>
            {
                // Right braces are not relevant; they are only a special character when in a left-brace quoted string
                new object[] { "[unbalanced brace" },
                new object[] { "'unbalanced apostrophe" },
                new object[] { "\"unbalanced quote" },
                new object[] { "/*unbalanced comment" },
            };

        private void AssertRegexMatches(string pattern, string input, string[] expected)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline);
            MatchCollection matches = regex.Matches(input);
            string[] actual = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                actual[i] = matches[i].Value;
            }
            CollectionAssert.AreEqual(expected, actual);
        }

        // SqlPart tokenizes the string into individual chunks of sql:
        // comments, strings, quoted identifiers, and unspecified (i.e strings of sql keywords)
        // Question mark parameters are ignored, but still cause a split
        [TestCaseSource(nameof(SqlPartTestInput))]
        public void SqlPartTest(string input, string[] expected)
        {
            AssertRegexMatches(NamedParametersExtensions.SqlPart, input, expected);
        }

        public static IEnumerable<object[]> SqlPartTestInput =>
            new List<object[]>
            {
                new object[] { "SELECT ? + ' ' + ? AS Value", new[] { "SELECT ", " + ", "' '", " + ", " AS Value" } },
                new object[] { "SELECT 1 FROM myTable WHERE myColumn = ?", new[] { "SELECT 1 FROM myTable WHERE myColumn = " } },
                new object[] { "SELECT 1 FROM myTable WHERE myColumn=?", new[] { "SELECT 1 FROM myTable WHERE myColumn=" } },
                new object[] { "SELECT 1, [some column ?] FROM myTable WHERE myColumn = ?", new[] { "SELECT 1, ", "[some column ?]", " FROM myTable WHERE myColumn = " } },
                new object[] { @"SELECT 1, ""What is going on ?"" FROM myTable WHERE myColumn = ?", new[] { @"SELECT 1, ", @"""What is going on ?""", @" FROM myTable WHERE myColumn = " } },
                new object[] { "SELECT 1, 'What is going on ?' FROM myTable WHERE myColumn = ?", new[] { "SELECT 1, ", "'What is going on ?'", " FROM myTable WHERE myColumn = " } },
                new object[] { "SELECT 1 FROM myTable WHERE myColumn1 = ? AND myColumn2 = ?", new[] { "SELECT 1 FROM myTable WHERE myColumn1 = ", " AND myColumn2 = " } },
                new object[] { "SELECT 1 FROM myTable WHERE myColumn1 = ? AND myColumn2 = 'A question?' AND myColumn3 = ?", new[] { "SELECT 1 FROM myTable WHERE myColumn1 = ", " AND myColumn2 = ", "'A question?'", " AND myColumn3 = " } },
                new object[] { "SELECT 1, 'What is going on ?', \"What is going on ?\", [What is going on ?] FROM myTable WHERE myColumn = ?", new[] { "SELECT 1, ", "'What is going on ?'", ", ", "\"What is going on ?\"", ", ", "[What is going on ?]", " FROM myTable WHERE myColumn = " } },
                new object[] { "SELECT 1, 'What is going on \"arggh\"', \"What is going on ?\" FROM myTable WHERE myColumn = ?", new[] { "SELECT 1, ", "'What is going on \"arggh\"'", ", ", "\"What is going on ?\"", " FROM myTable WHERE myColumn = " } },
                new object[] { "SELECT 1, 'What is going on \"arggh', \"What is going on ?\" FROM myTable WHERE myColumn = ?", new[] { "SELECT 1, ", "'What is going on \"arggh'", ", ", "\"What is going on ?\"", " FROM myTable WHERE myColumn = " } },
                new object[] { "SELECT 1, 'What is going on \"arggh?', \"What is going on ?\" FROM myTable WHERE myColumn = ?", new[] { "SELECT 1, ", "'What is going on \"arggh?'", ", ", "\"What is going on ?\"", " FROM myTable WHERE myColumn = " } },
                new object[] { "SELECT 1, 'What is going on \"arggh?\"', \"What is going on ?\" FROM myTable WHERE myColumn = ?", new[] { "SELECT 1, ", "'What is going on \"arggh?\"'", ", ", "\"What is going on ?\"", " FROM myTable WHERE myColumn = " } },
                new object[] { "EXEC myProc ?,?,?,?", new[] { "EXEC myProc ", ",", ",", "," } },
                new object[] { "EXEC myProc ?, ?, ?, ?", new[] { "EXEC myProc ", ", ", ", ", ", " } },
                new object[] { "ab?cdef", new[] { "ab", "cdef" } },
                new object[] { "?abcdef", new[] { "abcdef" } },
                new object[] { "abcdef", new[] { "abcdef" } },
                new object[] { "abcdef?", new[] { "abcdef" } },
                new object[] { "?abcdef?", new[] { "abcdef" } },
                new object[] { "ab?cd?ef", new[] { "ab", "cd", "ef" } },
                new object[] { "ab?c?d?ef", new[] { "ab", "c", "d", "ef" } },
                new object[] { "ab''cdef", new[] { "ab", "''", "cdef" } },
                new object[] { "ab''''cdef", new[] { "ab", "''''", "cdef" } },
                new object[] { "''abcdef", new[] { "''", "abcdef" } },
                new object[] { "abcdef''", new[] { "abcdef", "''" } },
                new object[] { "'abcdef'", new[] { "'abcdef'" } },
                new object[] { "'abc?def'", new[] { "'abc?def'" } },
                new object[] { "ab'cd'ef", new[] { "ab", "'cd'", "ef" } },
                new object[] { "ab'c''d'ef", new[] { "ab", "'c''d'", "ef" } },
                new object[] { "ab\"\"cdef", new[] { "ab", "\"\"", "cdef" } },
                new object[] { "ab\"\"\"\"cdef", new[] { "ab", "\"\"\"\"", "cdef" } },
                new object[] { "\"\"abcdef", new[] { "\"\"", "abcdef" } },
                new object[] { "abcdef\"\"", new[] { "abcdef", "\"\"" } },
                new object[] { "\"abcdef\"", new[] { "\"abcdef\"" } },
                new object[] { "\"abc?def\"", new[] { "\"abc?def\"" } },
                new object[] { "ab\"cd\"ef", new[] { "ab", "\"cd\"", "ef" } },
                new object[] { "ab\"c\"\"d\"ef", new[] { "ab", "\"c\"\"d\"", "ef" } },
                new object[] { "ab[]cdef", new[] { "ab", "[]", "cdef" } },
                new object[] { "ab[]]]cdef", new[] { "ab", "[]]]", "cdef" } },
                new object[] { "[]abcdef", new[] { "[]", "abcdef" } },
                new object[] { "abcdef[]", new[] { "abcdef", "[]" } },
                new object[] { "[abcdef]", new[] { "[abcdef]" } },
                new object[] { "[abc?def]", new[] { "[abc?def]" } },
                new object[] { "ab[cd]ef", new[] { "ab", "[cd]", "ef" } },
                new object[] { "ab[c]]d]ef", new[] { "ab", "[c]]d]", "ef" } },
                new object[] { "ab[c]]?d]ef", new[] { "ab", "[c]]?d]", "ef" } },
                new object[] { "abc/def", new[] { "abc/def" } },
                new object[] { "abc-def", new[] { "abc-def" } },
                new object[] { "--abcdef", new[] { "--abcdef" } },
                new object[] { "--abc?def", new[] { "--abc?def" } },
                new object[] { "-/-abcdef", new[] { "-/-abcdef" } },
                new object[] { "-/-abc?def", new[] { "-/-abc", "def" } },
                new object[] { "abc--def", new[] { "abc", "--def" } },
                new object[] { "abc-/-def", new[] { "abc-/-def" } },
                new object[] { "--abcdef\n", new[] { "--abcdef\n" } },
                new object[] { "abc--def\n", new[] { "abc", "--def\n" } },
                new object[] { "/*abcdef*/", new[] { "/*abcdef*/" } },
                new object[] { "/*abc?def*/", new[] { "/*abc?def*/" } },
                new object[] { "/-*abcdef*/", new[] { "/-*abcdef*/" } },
                new object[] { "/-*abc?def*/", new[] { "/-*abc", "def*/" } },
                new object[] { "/*abc//def*/ghi", new[] { "/*abc//def*/", "ghi" } },
                new object[] { "abc/*def\nghi*//jkl", new[] { "abc", "/*def\nghi*/", "/jkl" } },
                new object[] { "abc/*def\nghi*///jkl", new[] { "abc", "/*def\nghi*/", "//jkl" } },
                new object[] { "select 'abcdef', ?, 'ghijki', ? from foo", new[] { "select ", "'abcdef'", ", ", ", ", "'ghijki'", ", ", " from foo" } },
                new object[] { "select 'abc?def', ?, 'ghi?jki', ? from foo", new[] { "select ", "'abc?def'", ", ", ", ", "'ghi?jki'", ", ", " from foo" } },
                new object[] { "select 'abc\"def', ?, 'ghi\"jki', ? from foo", new[] { "select ", "'abc\"def'", ", ", ", ", "'ghi\"jki'", ", ", " from foo" } },
                new object[] { "select 'abc''def', ?, 'ghi''jki', ? from foo", new[] { "select ", "'abc''def'", ", ", ", ", "'ghi''jki'", ", ", " from foo" } },
                new object[] { "select 'abc[def', ?, 'ghi]jki', ? from foo", new[] { "select ", "'abc[def'", ", ", ", ", "'ghi]jki'", ", ", " from foo" } },
                new object[] { "select 'abc--def', ?, 'ghi--jki', ? from foo", new[] { "select ", "'abc--def'", ", ", ", ", "'ghi--jki'", ", ", " from foo" } },
                new object[] { "select 'abc//def', ?, 'ghi//jki', ? from foo", new[] { "select ", "'abc//def'", ", ", ", ", "'ghi//jki'", ", ", " from foo" } },
                new object[] { "select 'abc/*def', ?, 'ghi*/jki', ? from foo", new[] { "select ", "'abc/*def'", ", ", ", ", "'ghi*/jki'", ", ", " from foo" } },
                new object[] { "select \"abcdef\", ?, \"ghijki\", ? from foo", new[] { "select ", "\"abcdef\"", ", ", ", ", "\"ghijki\"", ", ", " from foo" } },
                new object[] { "select \"abc?def\", ?, \"ghi?jki\", ? from foo", new[] { "select ", "\"abc?def\"", ", ", ", ", "\"ghi?jki\"", ", ", " from foo" } },
                new object[] { "select \"abc[def\", ?, \"ghi]jki\", ? from foo", new[] { "select ", "\"abc[def\"", ", ", ", ", "\"ghi]jki\"", ", ", " from foo" } },
                new object[] { "select \"abc\"\"def\", ?, \"ghi\"\"jki\", ? from foo", new[] { "select ", "\"abc\"\"def\"", ", ", ", ", "\"ghi\"\"jki\"", ", ", " from foo" } },
                new object[] { "select \"abc'def\", ?, \"ghi'jki\", ? from foo", new[] { "select ", "\"abc'def\"", ", ", ", ", "\"ghi'jki\"", ", ", " from foo" } },
                new object[] { "select \"abc--def\", ?, \"ghi--jki\", ? from foo", new[] { "select ", "\"abc--def\"", ", ", ", ", "\"ghi--jki\"", ", ", " from foo" } },
                new object[] { "select \"abc//def\", ?, \"ghi//jki\", ? from foo", new[] { "select ", "\"abc//def\"", ", ", ", ", "\"ghi//jki\"", ", ", " from foo" } },
                new object[] { "select \"abc/*def\", ?, \"ghi*/jki\", ? from foo", new[] { "select ", "\"abc/*def\"", ", ", ", ", "\"ghi*/jki\"", ", ", " from foo" } },
                new object[] { "select [abcdef], ?, [ghijki], ? from foo", new[] { "select ", "[abcdef]", ", ", ", ", "[ghijki]", ", ", " from foo" } },
                new object[] { "select [abc?def], ?, [ghi?jki], ? from foo", new[] { "select ", "[abc?def]", ", ", ", ", "[ghi?jki]", ", ", " from foo" } },
                new object[] { "select [abc[def], ?, [ghi]]jki], ? from foo", new[] { "select ", "[abc[def]", ", ", ", ", "[ghi]]jki]", ", ", " from foo" } },
                new object[] { "select [abc\"def], ?, [ghi\"jki], ? from foo", new[] { "select ", "[abc\"def]", ", ", ", ", "[ghi\"jki]", ", ", " from foo" } },
                new object[] { "select [abc'def], ?, [ghi'jki], ? from foo", new[] { "select ", "[abc'def]", ", ", ", ", "[ghi'jki]", ", ", " from foo" } },
                new object[] { "select [abc--def], ?, [ghi--jki], ? from foo", new[] { "select ", "[abc--def]", ", ", ", ", "[ghi--jki]", ", ", " from foo" } },
                new object[] { "select [abc//def], ?, [ghi//jki], ? from foo", new[] { "select ", "[abc//def]", ", ", ", ", "[ghi//jki]", ", ", " from foo" } },
                new object[] { "select [abc/*def], ?, [ghi*/jki], ? from foo", new[] { "select ", "[abc/*def]", ", ", ", ", "[ghi*/jki]", ", ", " from foo" } },
                new object[] { "select /*abcdef*/, ?, /*ghijki*/, ? from foo", new[] { "select ", "/*abcdef*/", ", ", ", ", "/*ghijki*/", ", ", " from foo" } },
                new object[] { "select /*abc?def*/, ?, /*ghi?jki*/, ? from foo", new[] { "select ", "/*abc?def*/", ", ", ", ", "/*ghi?jki*/", ", ", " from foo" } },
                new object[] { "select /*abc\"def*/, ?, /*ghi\"jki*/, ? from foo", new[] { "select ", "/*abc\"def*/", ", ", ", ", "/*ghi\"jki*/", ", ", " from foo" } },
                new object[] { "select /*abc'def*/, ?, /*ghi'jki*/, ? from foo", new[] { "select ", "/*abc'def*/", ", ", ", ", "/*ghi'jki*/", ", ", " from foo" } },
                new object[] { "select /*abc[def*/, ?, /*ghi]jki*/, ? from foo", new[] { "select ", "/*abc[def*/", ", ", ", ", "/*ghi]jki*/", ", ", " from foo" } },
                new object[] { "select /*abc--def*/, ?, /*ghi--jki*/, ? from foo", new[] { "select ", "/*abc--def*/", ", ", ", ", "/*ghi--jki*/", ", ", " from foo" } },
                new object[] { "select /*abc//def*/, ?, /*ghi//jki*/, ? from foo", new[] { "select ", "/*abc//def*/", ", ", ", ", "/*ghi//jki*/", ", ", " from foo" } },
                new object[] { "select /*abc/*def*/, ?, /*ghi*/jki*/, ? from foo", new[] { "select ", "/*abc/*def*/", ", ", ", ", "/*ghi*/", "jki*/, ", " from foo" } },
                new object[] { "select ?, -- abcdef, ?, ghijkl, ? from foo", new[] { "select ", ", ", "-- abcdef, ?, ghijkl, ? from foo" } },
                new object[] { "select ?, -- abcdef, ?,\n ghijkl, ? from foo", new[] { "select ", ", ", "-- abcdef, ?,\n", " ghijkl, ", " from foo" } },
                new object[] { "select ?, // abcdef, ?, ghijkl, ? from foo", new[] { "select ", ", ", "// abcdef, ?, ghijkl, ? from foo" } },
                new object[] { "select ?, // abcdef, ?,\n ghijkl, ? from foo", new[] { "select ", ", ", "// abcdef, ?,\n", " ghijkl, ", " from foo" } },
                new object[] {
                    "select foo.*, bar.x/bar.y [xy?! ratio], bar.a-bar.b \"ab?! difference\", -- these names are awful, what are they for?\n"
                    + " coalesce(bar.g, '/*??*/') \"abc \"\" def\" /* using block comments with ? as a null placeholder value for legacy code handling */, "
                    + " ?/?+bar-? [[var1?]]], // why are all these names so bad?  why do they all contains ?\n"
                    + " ? / ? * bar.z - ? \"\"\"var2?\"\"\"--,\n"
                    + " /* // this seems unused? commenting out for testing \n ? / ? * bar.y - ? \"\"\"var3?\"\"\" */\n"
                    + " from [table1] [foo]\n"
                    + " left join \"table2\" \"bar\" on foo.k == bar.k\n"
                    + " where bar.l == ? and bar.e == 'no value'",
                    new[] {
                        "select foo.*, bar.x/bar.y ", "[xy?! ratio]", ", bar.a-bar.b ", "\"ab?! difference\"", ", ", "-- these names are awful, what are they for?\n",
                        " coalesce(bar.g, ", "'/*??*/'", ") ", "\"abc \"\" def\"", " ", "/* using block comments with ? as a null placeholder value for legacy code handling */",
                        ",  ", "/", "+bar-", " ", "[[var1?]]]", ", ", "// why are all these names so bad?  why do they all contains ?\n",
                        " ", " / ", " * bar.z - ", " ", "\"\"\"var2?\"\"\"", "--,\n",
                        " ", "/* // this seems unused? commenting out for testing \n ? / ? * bar.y - ? \"\"\"var3?\"\"\" */",
                        "\n from ", "[table1]", " ", "[foo]",
                        "\n left join ", "\"table2\"", " ", "\"bar\"",
                        " on foo.k == bar.k\n where bar.l == ", " and bar.e == ", "'no value'" }
                },
            };

        // SimpleSql extracts parts of the input string are are not special to our purposes,
        // i.e. everything that is not a comment, string, quoted identifier, or question mark
        [TestCaseSource(nameof(SimpleSqlPartTestInput))]
        public void SimpleSqlPartTest(string input, string[] expected)
        {
            AssertRegexMatches(NamedParametersExtensions.SimpleSqlPart, input, expected);
        }

        public static IEnumerable<object[]> SimpleSqlPartTestInput =>
            new List<object[]>
            {
                new object[] { "ab?cdef", new[] { "ab", "cdef" } },
                new object[] { "?abcdef", new[] { "abcdef" } },
                new object[] { "abcdef?", new[] { "abcdef" } },
                new object[] { "?abcdef?", new[] { "abcdef" } },
                new object[] { "ab?cd?ef", new[] { "ab", "cd", "ef" } },
                new object[] { "ab?c?d?ef", new[] { "ab", "c", "d", "ef" } },
                new object[] { "ab''cdef", new[] { "ab", "cdef" } },
                new object[] { "''abcdef", new[] { "abcdef" } },
                new object[] { "abcdef''", new[] { "abcdef" } },
                new object[] { "'abcdef'", new[] { "abcdef" } },
                new object[] { "ab'cd'ef", new[] { "ab", "cd", "ef" } },
                new object[] { "ab'c''d'ef", new[] { "ab", "c", "d", "ef" } },
                new object[] { "ab\"\"cdef", new[] { "ab", "cdef" } },
                new object[] { "\"\"abcdef", new[] { "abcdef" } },
                new object[] { "abcdef\"\"", new[] { "abcdef" } },
                new object[] { "\"abcdef\"", new[] { "abcdef" } },
                new object[] { "ab\"cd\"ef", new[] { "ab", "cd", "ef" } },
                new object[] { "ab\"c\"\"d\"ef", new[] { "ab", "c", "d", "ef" } },
                new object[] { "ab[]cdef", new[] { "ab", "]cdef" } },
                new object[] { "[]abcdef", new[] { "]abcdef" } },
                new object[] { "abcdef[]", new[] { "abcdef", "]" } },
                new object[] { "[abcdef]", new[] { "abcdef]" } },
                new object[] { "ab[cd]ef", new[] { "ab", "cd]ef" } },
                new object[] { "ab[c]]d]ef", new[] { "ab", "c]]d]ef" } },
                new object[] { "abc/def", new[] { "abc/def" } },
                new object[] { "abc-def", new[] { "abc-def" } },
                new object[] { "--abcdef", new[] { "-abcdef" } },
                new object[] { "-/-abcdef", new[] { "-/-abcdef" } },
                new object[] { "abc--def", new[] { "abc", "-def" } },
                new object[] { "abc-/-def", new[] { "abc-/-def" } },
                new object[] { "--abcdef\n", new[] { "-abcdef\n" } },
                new object[] { "abc--def\n", new[] { "abc", "-def\n" } },
                new object[] { "/*abcdef*/", new[] { "*abcdef*/" } },
                new object[] { "/-*abcdef*/", new[] { "/-*abcdef*/" } },
                new object[] { "/*abcdef*-/", new[] { "*abcdef*-/" } },
                new object[] { "/*abc//def*/ghi", new[] { "*abc", "/def*/ghi" } },
                new object[] { "abc/*def\nghi*//jkl", new[] { "abc", "*def\nghi*", "/jkl" } },
                new object[] { "abc/*def\nghi*///jkl", new[] { "abc", "*def\nghi*", "/jkl" } },
            };

        // Ansi quoted sql string uses 'apostrophes' at the start and end.
        // Double apostrophe 'foo''bar' is an escaped apostrophe within the string
        [TestCaseSource(nameof(AnsiStringTestInput))]
        public void AnsiStringTest(string input, string[] expected)
        {
            AssertRegexMatches(NamedParametersExtensions.AnsiString, input, expected);
        }

        public static IEnumerable<object[]> AnsiStringTestInput =>
            new List<object[]>
            {
                new object[] { "'abcdef", new string[0] },
                new object[] { "abcdef'", new string[0] },
                new object[] { "ab'cdef", new string[0] },
                new object[] { "ab'cdef", new string[0] },
                new object[] { "ab''cdef", new[] { "''" } },
                new object[] { "ab''''cdef", new[] { "''''" } },
                new object[] { "''abcdef", new[] { "''" } },
                new object[] { "abcdef''", new[] { "''" } },
                new object[] { "'abcdef'", new[] { "'abcdef'" } },
                new object[] { "ab'cd'ef", new[] { "'cd'" } },
                new object[] { "ab'c''d'ef", new[] { "'c''d'" } },
                new object[] { "ab'c'd'ef", new[] { "'c'" } },
            };

        // Ansi quoted identifier uses "quotes" at the start and end.
        // Double quotes "foo""bar" is an escaped quote within the identifier
        [TestCaseSource(nameof(AnsiQuotedIdentifierTestInput))]
        public void AnsiQuotedIdentifierTest(string input, string[] expected)
        {
            AssertRegexMatches(NamedParametersExtensions.AnsiQuotedIdentifier, input, expected);
        }

        public static IEnumerable<object[]> AnsiQuotedIdentifierTestInput =>
            new List<object[]>
            {
                new object[] { "\"abcdef", new string[0] },
                new object[] { "abcdef\"", new string[0] },
                new object[] { "ab\"cdef", new string[0] },
                new object[] { "ab\"cdef", new string[0] },
                new object[] { "ab\"\"cdef", new[] { "\"\"" } },
                new object[] { "\"\"abcdef", new[] { "\"\"" } },
                new object[] { "abcdef\"\"", new[] { "\"\"" } },
                new object[] { "\"abcdef\"", new[] { "\"abcdef\"" } },
                new object[] { "ab\"cd\"ef", new[] { "\"cd\"" } },
                new object[] { "ab\"c\"\"d\"ef", new[] { "\"c\"\"d\"" } },
                new object[] { "ab\"c\"d\"ef", new[] { "\"c\"" } },
            };

        // Square quoted identifier uses [square brackets] at the start and end.
        // Double right braket [foo]]bar] is an escaped square quote within the identifier
        [TestCaseSource(nameof(SquareQuotedIdentifierTestInput))]
        public void SquareQuotedIdentifierTest(string input, string[] expected)
        {
            AssertRegexMatches(NamedParametersExtensions.SquareQuotedIdentifier, input, expected);
        }

        public static IEnumerable<object[]> SquareQuotedIdentifierTestInput =>
            new List<object[]>
            {
                new object[] { "[abcdef", new string[0] },
                new object[] { "abcdef[", new string[0] },
                new object[] { "]abcdef", new string[0] },
                new object[] { "abcdef]", new string[0] },
                new object[] { "ab[cdef", new string[0] },
                new object[] { "ab]cdef", new string[0] },
                new object[] { "]abcdef[", new string[0] },
                new object[] { "ab[]cdef", new[] { "[]" } },
                new object[] { "[]abcdef", new[] { "[]" } },
                new object[] { "abcdef[]", new[] { "[]" } },
                new object[] { "[abcdef]", new[] { "[abcdef]" } },
                new object[] { "ab[cd]ef", new[] { "[cd]" } },
                new object[] { "ab[c]]d]ef", new[] { "[c]]d]" } },
                new object[] { "ab[c]d]ef", new[] { "[c]" } },
            };

        // Ansi comment is -- to the end of the line or input (whichever is first)
        [TestCaseSource(nameof(AnsiLineCommentTestInput))]
        public void AnsiLineCommentTest(string input, string[] expected)
        {
            AssertRegexMatches(NamedParametersExtensions.AnsiLineComment, input, expected);
        }

        public static IEnumerable<object[]> AnsiLineCommentTestInput =>
            new List<object[]>
            {
                new object[] { "--abcdef", new[] { "--abcdef" } },
                new object[] { "-/-abcdef", new string[0] },
                new object[] { "abc--def", new[] { "--def" } },
                new object[] { "abc-/-def", new string[0] },
                new object[] { "--abcdef\n", new[] { "--abcdef\n" } },
                new object[] { "abc--def\n", new[] { "--def\n" } },
                new object[] { "--abc\ndef--ghi", new[] { "--abc\n", "--ghi" } },
                new object[] { "abc--def\n--ghi", new[] { "--def\n", "--ghi" } },
                new object[] { "abc--def\nghi--jkl", new[] { "--def\n", "--jkl" } },
            };

        // C comment is one of:
        // Line comment // to the end of the line or input (whichever is first)
        // Block Comment /* until first */
        [TestCaseSource(nameof(CCommentTestInput))]
        public void CCommentTest(string input, string[] expected)
        {
            AssertRegexMatches(NamedParametersExtensions.CComment, input, expected);
        }

        public static IEnumerable<object[]> CCommentTestInput =>
            new List<object[]>
            {
                new object[] { "/-/abcdef", new string[0] },
                new object[] { "//abcdef", new[] { "//abcdef" } },
                new object[] { "abc//def", new[] { "//def" } },
                new object[] { "abc/-/def", new string[0] },
                new object[] { "//abcdef\n", new[] { "//abcdef\n" } },
                new object[] { "abc//def\n", new[] { "//def\n" } },
                new object[] { "//abc\ndef//ghi", new[] { "//abc\n", "//ghi" } },
                new object[] { "abc//def\n//ghi", new[] { "//def\n", "//ghi" } },
                new object[] { "abc//def\nghi//jkl", new[] { "//def\n", "//jkl" } },
                new object[] { "/*abcdef*/", new[] { "/*abcdef*/" } },
                new object[] { "/-*abcdef*/", new string[0] },
                new object[] { "/*abcdef*-/", new string[0] },
                new object[] { "/*abc//def*/ghi", new[] { "/*abc//def*/" } },
                new object[] { "abc/*def\nghi*//jkl", new[] { "/*def\nghi*/" } },
                new object[] { "abc/*def\nghi*///jkl", new[] { "/*def\nghi*/", "//jkl" } },
            };
    }
}
