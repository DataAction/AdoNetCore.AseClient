using System.Collections.Generic;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class ChangeDatabaseTests
    {
        private readonly IDictionary<string, string> _connectionStrings = ConnectionStringLoader.Load();

        [Test]
        public void ChangeDatabase_Success()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();
                connection.ChangeDatabase("tempdb");
                connection.ChangeDatabase("master");
            }
        }
    }
}
