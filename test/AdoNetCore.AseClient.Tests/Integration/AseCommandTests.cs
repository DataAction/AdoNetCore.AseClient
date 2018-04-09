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

                    var reader = command.ExecuteXmlReader();
                }
            }
        }
    }
}
