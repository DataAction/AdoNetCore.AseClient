using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    public partial class AseCommandTests
    {
        // IDbCommand.ExecuteNonQuery, Exceptions: InvalidOperationException, "The connection does not exist. -or- The connection is not open."
        [Test]
        public void Execute_WithoutAnOpenConnection_ThrowsInvalidOperation()
        {
            var noConnection = new AseCommand { CommandText = "select 1" };
            Assert.Throws<System.InvalidOperationException>(() => noConnection.ExecuteNonQuery());
            Assert.Throws<System.InvalidOperationException>(() => noConnection.ExecuteReader());
            Assert.Throws<System.InvalidOperationException>(() => noConnection.ExecuteScalar());

            var closed = new AseCommand("select 1", new AseConnection("Data Source=localhost;Port=5000;Uid=sa;Pwd=;"));
            Assert.Throws<System.InvalidOperationException>(() => closed.ExecuteNonQuery());
        }
    }
}
