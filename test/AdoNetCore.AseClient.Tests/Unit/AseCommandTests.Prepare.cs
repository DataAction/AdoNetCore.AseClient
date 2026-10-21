using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    public partial class AseCommandTests
    {
        // IDbCommand.Prepare, Exceptions: InvalidOperationException, "The Connection is not set. -or- The Connection is not Open()."
        [Test]
        public void Prepare_WithoutAnOpenConnection_ThrowsInvalidOperation()
        {
            Assert.Throws<System.InvalidOperationException>(() => new AseCommand("select 1").Prepare());

            var closed = new AseCommand("select 1", new AseConnection("Data Source=localhost;Port=5000;Uid=sa;Pwd=;"));
            Assert.Throws<System.InvalidOperationException>(() => closed.Prepare());
        }
    }
}
