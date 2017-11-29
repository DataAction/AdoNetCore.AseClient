using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class ChangeDatabaseTests
    {
        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

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
