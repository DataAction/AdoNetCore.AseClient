using System;
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
                    var configValue = result.GetValue(cConfigValue).ToString().Trim();

                    Assert.GreaterOrEqual(Convert.ToInt32(configValue), 100, "The database server should be configured to allow for more connections. Run the following: `exec sp_configure 'number of user connections', 100`");
                }
            }
        }
    }
}
