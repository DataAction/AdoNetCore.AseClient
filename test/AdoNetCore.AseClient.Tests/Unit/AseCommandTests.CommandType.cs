using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    public partial class AseCommandTests
    {
        // IDbCommand.CommandType: "The default is Text."
        [Test]
        public void CommandType_DefaultsToText()
        {
            Assert.AreEqual(System.Data.CommandType.Text, new AseCommand().CommandType);
        }
    }
}
