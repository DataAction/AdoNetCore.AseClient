using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class TestEnvironmentHealthTests
    {
        [Test]
        public void Adequate_UserConnections_Configuration()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "exec sp_configure 'number of user connections'";

                using (var result = command.ExecuteReader())
                {
                    result.Read();
                    var cConfigValue = result.GetOrdinal("Config Value");
                    var configValue = result.GetInt32(cConfigValue);

                    Assert.GreaterOrEqual(100, configValue, "The database should be configured to allow for more connections. Run the following: `exec sp_configure 'number of user connections', 100`");
                }
            }
        }

        [Test]
        public void Confirm_Disabled_UnicodeNormalization_Configuration()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "exec sp_configure 'enable unicode normalization'";

                using (var result = command.ExecuteReader())
                {
                    result.Read();
                    var cConfigValue = result.GetOrdinal("Config Value");
                    var configValue = result.GetInt32(cConfigValue);

                    Assert.AreEqual(0, configValue, "The database should be configured to disable unicode normalization. Run the following: `exec sp_configure 'enable unicode normalization', 0`");
                }
            }
        }
    }
}
