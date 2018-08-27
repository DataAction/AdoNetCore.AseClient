using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("basic")]
    public class ChangeDatabaseTests
    {
        [Test]
        public void ChangeDatabase_Success()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                connection.Open();
                connection.ChangeDatabase("tempdb");
                connection.ChangeDatabase("master");
            }
        }
    }
}
