using System;
using System.Collections.Generic;
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

        public static IEnumerable<TestCaseData> GetDataTypeName_ShouldWork_Cases()
        {
            if (CharsetUtility.IsCharset(ConnectionStrings.Pooled, "utf-8"))
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
    }
}
