using System.Collections.Generic;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class InternalConnectionTests
    {
        private readonly IDictionary<string, string> _connectionStrings = ConnectionStringLoader.Load();

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
