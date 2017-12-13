using System.Collections.Generic;
using System.IO;
using AdoNetCore.AseClient.Internal;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class InternalConnectionTests
    {
        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

        public InternalConnectionTests()
        {
            Logger.Enable();
        }

        [Test]
        public void Ping_ShouldWork()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]))
            {
                connection.Open();
                Assert.IsTrue(connection.InternalConnection.Ping());
            }
        }
    }
}
