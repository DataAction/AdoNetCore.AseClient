using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class AseConnectionPoolManagerTests
    {
        private readonly Dictionary<string, string> _connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ConnectionStrings.json"));

        [Test]
        public void NumberOfOpenConnections_NewConnectionWithUnpooledConnectionString_ReturnsZero()
        {
            var unpooledConnectionString = _connectionStrings["default"] + ";Pooling=false;UniqueID={BB44E31D-E62E-4F30-A340-D42E6258EB68}";
            var originalNumberOfOpenConnections = AseConnectionPoolManager.NumberOfOpenConnections;

            using (var connection = new AseConnection(unpooledConnectionString))
            {
                connection.Open();

                Assert.AreEqual(originalNumberOfOpenConnections, AseConnectionPoolManager.NumberOfOpenConnections);

                connection.Close();

            }

            Assert.AreEqual(originalNumberOfOpenConnections, AseConnectionPoolManager.NumberOfOpenConnections);
        }

        [Test]
        public void GetConnectionPool_NewConnectionWithUnpooledConnectionString_ReturnsPoolWithSizeZero()
        {
            var unpooledConnectionString = _connectionStrings["default"] + ";Pooling=false;UniqueID={62E59F3E-C4FF-4434-A1EE-17EF052F3206}";

            using (var connection = new AseConnection(unpooledConnectionString))
            {
                connection.Open();

                Assert.AreEqual(0, AseConnectionPoolManager.GetConnectionPool(unpooledConnectionString).Size);

                connection.Close();

            }

            Assert.AreEqual(0, AseConnectionPoolManager.GetConnectionPool(unpooledConnectionString).Size);
        }

        [Test]
        public void NumberOfOpenConnections_NewConnectionWithPooledConnectionString_ReturnsOne()
        {
            var unpooledConnectionString = _connectionStrings["default"] + ";Pooling=true;UniqueID={644A002D-FEDF-4F11-9AB3-A88709E7ED0A}";
            var originalNumberOfOpenConnections = AseConnectionPoolManager.NumberOfOpenConnections;

            using (var connection = new AseConnection(unpooledConnectionString))
            {
                connection.Open();

                Assert.AreEqual(originalNumberOfOpenConnections + 1, AseConnectionPoolManager.NumberOfOpenConnections);

                connection.Close();

            }

            Assert.AreEqual(originalNumberOfOpenConnections + 1, AseConnectionPoolManager.NumberOfOpenConnections);
        }

        [Test]
        public void GetConnectionPool_NewConnectionWithPooledConnectionString_ReturnsPoolWithSizeOne()
        {
            var unpooledConnectionString = _connectionStrings["default"] + ";Pooling=true;UniqueID={C43098E5-3708-4E1A-A58D-B1C77D5C94C3}";

            using (var connection = new AseConnection(unpooledConnectionString))
            {
                connection.Open();

                Assert.AreEqual(1, AseConnectionPoolManager.GetConnectionPool(unpooledConnectionString).Size);

                connection.Close();

            }

            Assert.AreEqual(1, AseConnectionPoolManager.GetConnectionPool(unpooledConnectionString).Size);
        }

        [Test]
        public void GetConnectionPool_NewConnectionWithPooledConnectionString_ReturnsPoolWithAvailable()
        {
            var unpooledConnectionString = _connectionStrings["default"] + ";Pooling=true;UniqueID={5BDD8727-BA64-46AF-B933-9EEB70E6E322}";

            using (var connection = new AseConnection(unpooledConnectionString))
            {
                connection.Open();

                Assert.AreEqual(0, AseConnectionPoolManager.GetConnectionPool(unpooledConnectionString).Available);

                connection.Close();

            }

            Assert.AreEqual(1, AseConnectionPoolManager.GetConnectionPool(unpooledConnectionString).Available);
        }
    }
}
