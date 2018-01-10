using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class ChangeDatabaseTests
    {
        [Test]
        public void ChangeDatabase_Success()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();
                connection.ChangeDatabase("tempdb");
                connection.ChangeDatabase("master");
            }
        }
    }
}
