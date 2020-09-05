using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using AdoNetCore.AseClient.Internal;
using AdoNetCore.AseClient.Tests.Util;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("basic")]
    public class AseCommandTests
    {
        private AseConnection GetConnection()
        {
            Logger.Enable();
            return new AseConnection(ConnectionStrings.Pooled);
        }

        private string xmlContent = @"<?xml version=""1.0""?>
<catalog>
  <book id=""bk101"">
    <author>Gambardella, Matthew</author>
  </book>
</catalog>";

        [Test]
        public void ExecuteXmlReader_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"select '{xmlContent}' as xml_content";

                    var doc = new XmlDocument();
                    using (var reader = command.ExecuteXmlReader())
                    {
                        doc.Load(reader);
                    }
                }
            }
        }

        [Test]
        public void ExecuteXmlReader_WithNonString_ThrowsAseException()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"select 1 as not_xml_content";
                    var ex = Assert.Throws<AseException>(() => command.ExecuteXmlReader());
                    Assert.AreEqual(30081, ex.Errors[0].MessageNumber);
                    Assert.AreEqual("Column type cannot hold xml data.", ex.Errors[0].Message);
                }
            }
        }

        [TestCaseSource(nameof(GetDataTypeName_InvalidDataOrParameters_ThrowsIndexOutOfRangeException_Cases))]
        public void GetDataTypeName_InvalidDataOrParameters_ThrowsIndexOutOfRangeException(string commandText, int index, string expectedMessage)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    var ex = Assert.Throws<IndexOutOfRangeException>(() => command.GetDataTypeName(index));
                    Assert.AreEqual(expectedMessage, ex.Message);
                }
            }
        }

        public static IEnumerable<TestCaseData> GetDataTypeName_InvalidDataOrParameters_ThrowsIndexOutOfRangeException_Cases()
        {
            yield return new TestCaseData("declare @i int", 0, "Column referenced by index (0) does not exist");
            yield return new TestCaseData("select 1", -1, "Column referenced by index (-1) does not exist");
        }

        [TestCaseSource(nameof(GetDataTypeName_InvalidDataOrParameters_ThrowsAseException_Cases))]
        public void GetDataTypeName_InvalidDataOrParameters_ThrowsAseException(string commandText, int index, int expectedMessageNumber, string expectedMessage)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    var ex = Assert.Throws<AseException>(() => command.GetDataTypeName(index));

                    Assert.AreEqual(expectedMessageNumber, ex.Errors[0].MessageNumber);
                    Assert.AreEqual(expectedMessage, ex.Errors[0].Message);
                }
            }
        }

        public static IEnumerable<TestCaseData> GetDataTypeName_InvalidDataOrParameters_ThrowsAseException_Cases()
        {
            yield return new TestCaseData("select 1", 2, 30118, "The column specified does not exist.");
        }

        [TestCaseSource(nameof(GetDataTypeName_ShouldWork_Cases))]
        public void GetDataTypeName_ShouldWork(string input, string expectedName)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"select {input}";
                    Assert.AreEqual(expectedName, command.GetDataTypeName(0));
                }
            }
        }

        [TestCaseSource(nameof(GetDataTypeName_ShouldWorkUtf8_Cases))]
        public void GetDataTypeName_ShouldWorkUtf8(string input, string expectedName)
        {
            if (!CharsetUtility.IsCharset(ConnectionStrings.Pooled, "utf-8"))
            {
                Assert.Ignore("Not run unless the server charset is UTF8");
            }
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"select {input}";
                    Assert.AreEqual(expectedName, command.GetDataTypeName(0));
                }
            }
        }

        [Test]
        public void Command_Reuse_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    {
                        var sql = @"SELECT TOP 1 Convert(Decimal(29,10), @value)";
                        var p = cmd.CreateParameter();
                        p.ParameterName = "@value";
                        p.DbType = System.Data.DbType.Decimal;
                        p.Value = 6579.64648m;
                        cmd.CommandText = sql;
                        cmd.Parameters.Add(p);

                        // 6579.64648m sent to server
                        using (var rd = cmd.ExecuteReader(System.Data.CommandBehavior.Default))
                        {
                            rd.Read();
                            var result = rd.GetDecimal(0);
                            Assert.AreEqual(6579.64648M, result);
                        }

                        p = cmd.CreateParameter();
                        p.ParameterName = "@value";
                        p.DbType = System.Data.DbType.Single;
                        p.Value = 6579.64648f;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(p);

                        // decimal formatter applied to 6579.64648f and returned value is 65.79....
                        using (var rd = cmd.ExecuteReader(System.Data.CommandBehavior.Default))
                        {
                            rd.Read();
                            var result = rd.GetDecimal(0);
                            Assert.AreEqual(6579.646484375M, result);
                        }
                    }
                }
            }
        }

        [TestCaseSource(nameof(ReUseCommandTypeData))]
        public void Command_Reuse_ShouldWork_For_Null_Value(System.Data.DbType dbType, Type type, object value)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    var sql = @"SELECT @value";
                    var p = cmd.CreateParameter();
                    p.ParameterName = "@value";
                    p.DbType = dbType;
                    p.Value = Convert.ChangeType(value, type);
                    cmd.CommandText = sql;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteScalar();

                    p = cmd.CreateParameter();
                    p.ParameterName = "@value";
                    p.DbType = dbType;
                    p.Value = null;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(p);

                    cmd.ExecuteScalar();
                }
            }
        }

        
        [TestCaseSource(nameof(ReUseCommandTypeData))]
        public void Command_Reuse_ShouldWork_For_NonNull_Value(System.Data.DbType dbType, Type type, object value)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    var sql = @"SELECT @value";
                    var p = cmd.CreateParameter();
                    p.ParameterName = "@value";
                    p.DbType = dbType;
                    p.Value = null;
                    cmd.CommandText = sql;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteScalar();

                    p = cmd.CreateParameter();
                    p.ParameterName = "@value";
                    p.DbType = dbType;
                    p.Value = Convert.ChangeType(value, type);
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(p);

                    cmd.ExecuteScalar();
                }
            }
        }

        [Test]
        public void Command_Duplicate_Parameter_Name_Should_Throws_ArgumentException()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var sql = "select @p1, @p2";
                var aseCommand = connection.CreateCommand();
                aseCommand.CommandText = sql;
                aseCommand.CommandType = CommandType.Text;
                aseCommand.Parameters.Add("@p1", "test");
                aseCommand.Parameters.Add("@p2", "test2");
                Assert.Throws<ArgumentException>(() => aseCommand.Parameters.Add("@p1", "test3"));
            }
        }

        private static IEnumerable<TestCaseData> ReUseCommandTypeData()
        {
            yield return new TestCaseData(DbType.Boolean, typeof(bool), false);
            yield return new TestCaseData(DbType.Byte, typeof(byte), 12);
            yield return new TestCaseData(DbType.SByte, typeof(sbyte), 2);
            yield return new TestCaseData(DbType.Int16, typeof(short), 3);
            yield return new TestCaseData(DbType.UInt16, typeof(ushort), 4);
            yield return new TestCaseData(DbType.Int32, typeof(int), 23);
            yield return new TestCaseData(DbType.UInt32, typeof(uint), 45);
            yield return new TestCaseData(DbType.Int64, typeof(long), 76434755);
            yield return new TestCaseData(DbType.UInt64, typeof(ulong), 1223);
            yield return new TestCaseData(DbType.String, typeof(string), "If it could only be like this always—always summer, always alone, the fruit always ripe");
            yield return new TestCaseData(DbType.AnsiString, typeof(string), "Doubt thou the stars are fire; Doubt that the sun doth move; Doubt truth to be a liar; But never doubt I love");
            yield return new TestCaseData(DbType.AnsiStringFixedLength, typeof(string), "For never was a story of more woe than this of Juliet and her Romeo.");
            yield return new TestCaseData(DbType.Guid, typeof(string), "e2207b47-3fce-4187-808f-e206398a9133");
            yield return new TestCaseData(DbType.Decimal, typeof(decimal), 342.23);
            yield return new TestCaseData(DbType.Currency, typeof(decimal), 1233.3);
            yield return new TestCaseData(DbType.Single, typeof(float), 20.34f);
            yield return new TestCaseData(DbType.Double, typeof(double), 3423.234d);
            yield return new TestCaseData(DbType.DateTime, typeof(DateTime), "2019-03-13 03:20:35.23 AM");
            yield return new TestCaseData(DbType.Date, typeof(DateTime), "2018-07-04 23:20:35.23 PM");
            yield return new TestCaseData(DbType.Time, typeof(DateTime), "2014-09-10 23:20:35");

        }

        private static IEnumerable<TestCaseData> GetDataTypeName_ShouldWork_Cases()
        {
            yield return new TestCaseData("convert(unichar(2), 'À')", "unichar");
            yield return new TestCaseData("convert(unichar(2), null)", "unichar");
            yield return new TestCaseData("convert(univarchar(2), 'a')", "univarchar");
            yield return new TestCaseData("convert(univarchar(2), null)", "univarchar");
            yield return new TestCaseData("convert(text, 'a')", "text");
            yield return new TestCaseData("convert(text, null)", "text");
            yield return new TestCaseData("convert(unitext, 'a')", "unitext");
            yield return new TestCaseData("convert(unitext, null)", "unitext");
            yield return new TestCaseData("convert(image, 0xFF)", "image");
            yield return new TestCaseData("convert(image, null)", "image");
            yield return new TestCaseData("convert(binary, 0xFF)", "binary");
            yield return new TestCaseData("convert(binary, null)", "binary");
            yield return new TestCaseData("convert(varbinary, 0xFF)", "varbinary");
            yield return new TestCaseData("convert(varbinary, null)", "varbinary");
            yield return new TestCaseData("convert(tinyint, 1)", "tinyint");
            yield return new TestCaseData("convert(tinyint, null)", "tinyint");
            yield return new TestCaseData("convert(smallint, 1)", "smallint");
            yield return new TestCaseData("convert(smallint, null)", "smallint");
            yield return new TestCaseData("convert(int, 1)", "int");
            yield return new TestCaseData("convert(int, null)", "int");
            yield return new TestCaseData("convert(bigint, 1)", "bigint");
            yield return new TestCaseData("convert(bigint, null)", "bigint");
            yield return new TestCaseData("convert(bit, 1)", "bit");
            yield return new TestCaseData("convert(unsigned tinyint, 1)", "tinyint");
            yield return new TestCaseData("convert(unsigned tinyint, null)", "tinyint");
            yield return new TestCaseData("convert(unsigned smallint, 1)", "unsigned smallint");
            yield return new TestCaseData("convert(unsigned smallint, null)", "unsigned smallint");
            yield return new TestCaseData("convert(unsigned int, 1)", "unsigned int");
            yield return new TestCaseData("convert(unsigned int, null)", "unsigned int");
            yield return new TestCaseData("convert(unsigned bigint, 1)", "unsigned bigint");
            yield return new TestCaseData("convert(unsigned bigint, null)", "unsigned bigint");
            yield return new TestCaseData("convert(datetime, getdate())", "datetime");
            yield return new TestCaseData("convert(datetime, null)", "datetime");
            yield return new TestCaseData("convert(smalldatetime, getdate())", "smalldatetime");
            yield return new TestCaseData("convert(smalldatetime, null)", "smalldatetime");
            yield return new TestCaseData("convert(time, getdate())", "time");
            yield return new TestCaseData("convert(time, null)", "time");
            yield return new TestCaseData("convert(date, getdate())", "date");
            yield return new TestCaseData("convert(date, null)", "date");
            yield return new TestCaseData("convert(money, 1)", "money");
            yield return new TestCaseData("convert(money, null)", "money");
            yield return new TestCaseData("convert(smallmoney, 1)", "smallmoney");
            yield return new TestCaseData("convert(smallmoney, null)", "smallmoney");
            yield return new TestCaseData("convert(decimal(10, 5), 1)", "decimal");
            yield return new TestCaseData("convert(decimal(10, 5), null)", "decimal");
            yield return new TestCaseData("convert(numeric(10, 5), 1)", "numeric");
            yield return new TestCaseData("convert(numeric(10, 5), null)", "numeric");
            yield return new TestCaseData("convert(real, 1)", "real");
            yield return new TestCaseData("convert(real, null)", "real");
            yield return new TestCaseData("convert(float, 1)", "float");
            yield return new TestCaseData("convert(float, null)", "float");
        }

        private static IEnumerable<TestCaseData> GetDataTypeName_ShouldWorkUtf8_Cases()
        {
            yield return new TestCaseData("convert(char(1), 'a')", "char");
            yield return new TestCaseData("convert(char(1), null)", "char");
            yield return new TestCaseData("convert(nchar(2), 'À')", "char");
            yield return new TestCaseData("convert(nchar(2), null)", "char");
            yield return new TestCaseData("convert(varchar(1), 'a')", "varchar");
            yield return new TestCaseData("convert(varchar(1), null)", "varchar");
            yield return new TestCaseData("convert(nvarchar(2), 'a')", "nvarchar");
            yield return new TestCaseData("convert(nvarchar(2), null)", "nvarchar");
        }

        public T CastObject<T>(object input) {   
            return (T) input;   
        }

        public T ConvertObject<T>(object input) {
            return (T) Convert.ChangeType(input, typeof(T));
        }

    }

}
