using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class LoginTests
    {
        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

        [TestCase("default")]
        [TestCase("big-packetsize")]
        public void Login_Success(string csName)
        {
            using (var connection = new AseConnection(_connectionStrings[csName]))
            {
                connection.Open();
                Assert.IsTrue(connection.State == ConnectionState.Open, "Connection state should be Open after calling Open()");
            }
        }

        [Test]
        public void Login_Failure()
        {
            using (var connection = new AseConnection(_connectionStrings["badpass"]))
            {
                Assert.Throws<AseException>(() => connection.Open());
            }
        }

        [Test]
        public void CannotResolveServer_Failure()
        {
            using (var connection = new AseConnection("Data Source=myASEServer;Port=5000;Database=mydb;Uid=x;Pwd=y;"))
            {
                Assert.Throws<SocketException>(() => connection.Open());
            }
        }

        [Test]
        [Ignore("Passes sometimes, depends on cancellation token reliability")]
        public void SmallTimeout_Failure()
        {
            using (var connection = new AseConnection(_connectionStrings["default"]+";LoginTimeoutMs=1"))
            {
                Assert.Throws<AseException>(() => connection.Open());
            }
        }
    }
}
