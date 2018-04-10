using System.Xml;
using AdoNetCore.AseClient.Internal;
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
    }
}
