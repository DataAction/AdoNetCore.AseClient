using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class InternalConnectionTests
    {
        public InternalConnectionTests()
        {
            Logger.Enable();
        }

        [Test]
        public void Ping_ShouldWork()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();
                Assert.IsTrue(connection.InternalConnection.Ping());
            }
        }
    }
}
