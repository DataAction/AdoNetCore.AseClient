using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Query
{
    /// <summary>
    /// NOTE: these tests rely on the server encoding (UTF-8). Behaviour is consistent when run against the reference driver.
    /// </summary>
    [Category("basic")]
#if NET_FRAMEWORK
    [TestFixture(typeof(SapConnectionProvider))]
#endif
    [TestFixture(typeof(CoreFxConnectionProvider))]
    public class StringTests<T> where T : IConnectionProvider
    {
        private DbConnection GetConnection()
        {
            return Activator.CreateInstance<T>().GetConnection(ConnectionStrings.Pooled);
        }

        [SetUp]
        public void Setup()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("create table [dbo].[nchar_test_table] (value nvarchar(10))");
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("drop table [dbo].[nchar_test_table]");
            }
        }

        /// <summary>
        /// From old StringTests.cs
        /// </summary>
        [Test]
        public void CharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(char(3), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        /// <summary>
        /// From old StringTests.cs
        /// </summary>
        [Test]
        public void VarcharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(varchar(10), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        /// <summary>
        /// From old StringTests.cs
        /// </summary>
        [Test]
        public void NcharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(nchar(1), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        /// <summary>
        /// From old StringTests.cs
        /// </summary>
        [Test]
        public void NvarcharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(nvarchar(10), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        /// <summary>
        /// From old StringTests.cs
        /// </summary>
        [Test]
        public void TextEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(text, 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        /// <summary>
        /// From old StringTests.cs
        /// </summary>
        [Test]
        public void UnicharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(unichar(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        /// <summary>
        /// From old StringTests.cs
        /// </summary>
        [Test]
        public void UnivarcharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(univarchar(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        /// <summary>
        /// From old StringTests.cs
        /// </summary>
        [Test]
        public void UnitextEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(unitext, 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        /// <summary>
        /// From old StringTests.cs
        /// </summary>
        [TestCase("select '['+@input+']'", "", "[ ]")]
        [TestCase("select @input", "", " ")]
        [TestCase("select convert(char, @input)", null, null)]
        [TestCase("select '['+convert(char, @input)+']'", null, "[]")]
        [TestCase("select convert(char, '['+@input+']')", null, "[]                            ")]
        public void Select_StringParameter(string sql, object input, object expected)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    var p = command.CreateParameter();
                    p.ParameterName = "@input";
                    p.Value = input ?? DBNull.Value;
                    p.DbType = DbType.AnsiString;
                    command.Parameters.Add(p);

                    Assert.AreEqual(expected ?? DBNull.Value, command.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// From old StringTests.cs
        /// </summary>
        [TestCase("select ''", " ")]
        [TestCase("select convert(char, '')", "                              ")]
        [TestCase("select convert(char(1), '')", " ")]
        [TestCase("select convert(nchar(1), '')", "   ")]
        [TestCase("select convert(unichar(1), '')", " ")]
        [TestCase("select convert(varchar(1), '')", " ")]
        [TestCase("select convert(univarchar(1), '')", " ")]
        [TestCase("select convert(nvarchar(1), '')", " ")]
        [TestCase("select convert(char, null)", null)]
        [TestCase("select convert(char(1), null)", null)]
        [TestCase("select convert(unichar(1), null)", null)]
        [TestCase("select convert(nchar(1), null)", null)]
        [TestCase("select convert(varchar(1), null)", null)]
        [TestCase("select convert(univarchar(1), null)", null)]
        [TestCase("select convert(nvarchar(1), null)", null)]
        public void Select_StringLiteral(string sql, object expected)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    var result = command.ExecuteScalar();
                    Console.WriteLine($"[{result}]");
                    Assert.AreEqual(expected ?? DBNull.Value, result);
                }
            }
        }

        [TestCaseSource(nameof(SelectNChar_SingleChar_Param_Cases))]
        public void SelectNChar_SingleChar_Param(string _, char input, char expected)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@input", input, DbType.String);
                var result = connection.QuerySingle<char>("select @input", p);
                Assert.AreEqual(expected, result, $"Expected: '{expected}' ({(int)expected}); Result: '{result}' ({(int)result})");
            }
        }

        public static IEnumerable<TestCaseData> SelectNChar_SingleChar_Param_Cases()
        {
            yield return new TestCaseData("\\0", '\0', ' ');
            yield return new TestCaseData("\\x09", '\x09', '\x09');
            yield return new TestCaseData("\\x0A", '\x0A', '\x0A');
            yield return new TestCaseData("\\x0B", '\x0B', '\x0B');
            yield return new TestCaseData("\\x0C", '\x0C', '\x0C');
            yield return new TestCaseData("\\x0D", '\x0D', '\x0D');
            yield return new TestCaseData("\\xA0", '\xA0', '\xA0');
            yield return new TestCaseData("space", ' ', ' ');
            yield return new TestCaseData("u 2000", '\u2000', '\u2002');
            yield return new TestCaseData("u 2001", '\u2001', '\u2003');
            yield return new TestCaseData("u 2002", '\u2002', '\u2002');
            yield return new TestCaseData("u 2003", '\u2003', '\u2003');
            yield return new TestCaseData("u 2004", '\u2004', '\u2004');
            yield return new TestCaseData("u 2005", '\u2005', '\u2005');
            yield return new TestCaseData("u 2006", '\u2006', '\u2006');
            yield return new TestCaseData("u 2007", '\u2007', '\u2007');
            yield return new TestCaseData("u 2008", '\u2008', '\u2008');
            yield return new TestCaseData("u 2009", '\u2009', '\u2009');
            yield return new TestCaseData("u 200A", '\u200A', '\u200A');
            yield return new TestCaseData("u 3000", '\u3000', '\u3000');
        }

        [TestCaseSource(nameof(SelectNChar_String_Param_Cases))]
        public void SelectNChar_String_Param(string _, string input, string expected)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@input", input, DbType.String);
                var result = connection.QuerySingle<string>("select @input", p);
                Assert.AreEqual(expected, result);
            }
        }

        public static IEnumerable<TestCaseData> SelectNChar_String_Param_Cases()
        {
            yield return new TestCaseData("\\0 end", "test\0", "test");
            yield return new TestCaseData("\\0 mid", "te\0st", "te");
            yield return new TestCaseData("\\0 start", "\0test", " ");
            yield return new TestCaseData("\test\x09", "test\x09", "test\x09");
            yield return new TestCaseData("\test\x0A", "test\x0A", "test\x0A");
            yield return new TestCaseData("\test\x0B", "test\x0B", "test\x0B");
            yield return new TestCaseData("\test\x0C", "test\x0C", "test\x0C");
            yield return new TestCaseData("\test\x0D", "test\x0D", "test\x0D");
            yield return new TestCaseData("\test\xA0", "test\xA0", "test\xA0");
            yield return new TestCaseData("space", "test ", "test ");
            yield return new TestCaseData("u 2000", "test\u2000", "test\u2002");
            yield return new TestCaseData("u 2001", "test\u2001", "test\u2003");
            yield return new TestCaseData("u 2002", "test\u2002", "test\u2002");
            yield return new TestCaseData("u 2003", "test\u2003", "test\u2003");
            yield return new TestCaseData("u 2004", "test\u2004", "test\u2004");
            yield return new TestCaseData("u 2005", "test\u2005", "test\u2005");
            yield return new TestCaseData("u 2006", "test\u2006", "test\u2006");
            yield return new TestCaseData("u 2007", "test\u2007", "test\u2007");
            yield return new TestCaseData("u 2008", "test\u2008", "test\u2008");
            yield return new TestCaseData("u 2009", "test\u2009", "test\u2009");
            yield return new TestCaseData("u 200A", "test\u200A", "test\u200A");
            yield return new TestCaseData("u 3000", "test\u3000", "test\u3000");
        }

        [TestCaseSource(nameof(SelectNChar_FromTable_Cases))]
        public void SelectNChar_FromTable(string _, string input, string expected)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@input", input, DbType.String);
                connection.Execute("insert into [dbo].[nchar_test_table] (value) values (@input)", p);
            }

            using (var connection = GetConnection())
            {
                Assert.AreEqual(expected, connection.QuerySingle<string>("select top 1 value from [dbo].[nchar_test_table]"));
            }
        }

        public static IEnumerable<TestCaseData> SelectNChar_FromTable_Cases()
        {
            yield return new TestCaseData("\\0 end", "test\0", "test");
            yield return new TestCaseData("\\0 mid", "te\0st", "te");
            yield return new TestCaseData("\\0 start", "\0test", " ");
            yield return new TestCaseData("\\0", "\0", " ");
            yield return new TestCaseData("\\x09", "\x09", "\x09");
            yield return new TestCaseData("\\x0A", "\x0A", "\x0A");
            yield return new TestCaseData("\\x0B", "\x0B", "\x0B");
            yield return new TestCaseData("\\x0C", "\x0C", "\x0C");
            yield return new TestCaseData("\\x0D", "\x0D", "\x0D");
            yield return new TestCaseData("\\xA0", "\xA0", "\xA0");
            yield return new TestCaseData("\test\x09", "test\x09", "test\x09");
            yield return new TestCaseData("\test\x0A", "test\x0A", "test\x0A");
            yield return new TestCaseData("\test\x0B", "test\x0B", "test\x0B");
            yield return new TestCaseData("\test\x0C", "test\x0C", "test\x0C");
            yield return new TestCaseData("\test\x0D", "test\x0D", "test\x0D");
            yield return new TestCaseData("\test\xA0", "test\xA0", "test\xA0");
            yield return new TestCaseData("space", " ", " ");
            yield return new TestCaseData("space", "test ", "test");
            yield return new TestCaseData("u 2000", "\u2000", "\u2002");
            yield return new TestCaseData("u 2000", "test\u2000 ", "test\u2002");
            yield return new TestCaseData("u 2001", "\u2001", "\u2003");
            yield return new TestCaseData("u 2001", "test\u2001", "test\u2003");
            yield return new TestCaseData("u 2002", "\u2002", "\u2002");
            yield return new TestCaseData("u 2002", "test\u2002", "test\u2002");
            yield return new TestCaseData("u 2003", "\u2003", "\u2003");
            yield return new TestCaseData("u 2003", "test\u2003", "test\u2003");
            yield return new TestCaseData("u 2004", "\u2004", "\u2004");
            yield return new TestCaseData("u 2004", "test\u2004", "test\u2004");
            yield return new TestCaseData("u 2005", "\u2005", "\u2005");
            yield return new TestCaseData("u 2005", "test\u2005", "test\u2005");
            yield return new TestCaseData("u 2006", "\u2006", "\u2006");
            yield return new TestCaseData("u 2006", "test\u2006", "test\u2006");
            yield return new TestCaseData("u 2007", "\u2007", "\u2007");
            yield return new TestCaseData("u 2007", "test\u2007", "test\u2007");
            yield return new TestCaseData("u 2008", "\u2008", "\u2008");
            yield return new TestCaseData("u 2008", "test\u2008", "test\u2008");
            yield return new TestCaseData("u 2009", "\u2009", "\u2009");
            yield return new TestCaseData("u 2009", "test\u2009", "test\u2009");
            yield return new TestCaseData("u 200A", "\u200A", "\u200A");
            yield return new TestCaseData("u 200A", "test\u200A", "test\u200A");
            yield return new TestCaseData("u 3000", "\u3000", "\u3000");
            yield return new TestCaseData("u 3000", "test\u3000", "test\u3000");
        }
    }
}
