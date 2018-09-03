using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("extra")]
    public class AseConnectionPoolManagerTests
    {
        [Test]
        public void NumberOfOpenConnections_NewConnectionWithUnpooledConnectionString_ReturnsZero()
        {
            var unpooledConnectionString = ConnectionStrings.NonPooledUnique;
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
            var unpooledConnectionString = ConnectionStrings.NonPooledUnique;

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
            var unpooledConnectionString = ConnectionStrings.PooledUnique;
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
            var unpooledConnectionString = ConnectionStrings.PooledUnique;

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
            var unpooledConnectionString = ConnectionStrings.PooledUnique;

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
