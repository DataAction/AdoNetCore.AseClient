using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("basic")]
    public class ConnectionTests
    {
        public ConnectionTests()
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
        
        [Test]
        public void OpenConnection_ToNonExistantServer_ThrowsAseException()
        {
            using (var connection = new AseConnection("Data Source=NOTHING_INTERESTING_HERE; Port=5000; Uid=uid; Pwd=pwd; db=db;"))
            {
                var ex = Assert.Throws<AseException>(() => connection.Open());
                Assert.AreEqual("Client unable to establish a connection", ex.Message);
                Assert.AreEqual(30010, ex.Errors[0].MessageNumber);
            }
        }

        [Test]
        public void OpenConnection_ToNonListeningServer_ThrowsAseException()
        {
            using (var connection = new AseConnection("Data Source=localhost; Port=54321; Uid=uid; Pwd=pwd; db=db;"))
            {
                var ex = Assert.Throws<AseException>(() => connection.Open());
                Assert.AreEqual("There is no server listening at localhost:54321.", ex.Message);
                Assert.AreEqual(30294, ex.Errors[0].MessageNumber);
            }
        }
    }
}
