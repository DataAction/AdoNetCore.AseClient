using System;
using System.Data;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;
using Moq;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    public class AseConnectionTests
    {
        [Test]
        public void ConstructConnection_WithNoArgs_NoErrors()
        {
            var unused = new AseConnection();

            Assert.Pass();
        }

        [Test]
        public void ConstructConnection_WithEmptyArgs_NoErrors()
        {
            var unused = new AseConnection(string.Empty);

            Assert.Pass();
        }

        [Test]
        public void ConstructConnection_WithValidConnectionString_NoErrors()
        {
            var unused =
                new AseConnection(
                    "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;");

            Assert.Pass();
        }


        [Test]
        public void OpenConnection_WithInvalidConnectionString_ThrowsArgumentException()
        {
            var connection = new AseConnection(string.Empty);

            Assert.Throws<ArgumentException>(() => { connection.Open(); });
        }

        [Test]
        public void OpenConnection_WithValidConnectionString_OpensConnection()
        {
            // Arrange
            var mockConnection = new Mock<IInternalConnection>();
            var mockConnectionPoolManager = new Mock<IConnectionPoolManager>();
            mockConnectionPoolManager.Setup(x => x.Reserve(It.IsAny<string>(), It.IsAny<ConnectionParameters>()))
                .Returns(mockConnection.Object);

            var connection =
                new AseConnection(
                    "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;",
                    mockConnectionPoolManager.Object);

            // Act
            connection.Open();

            // Assert
            Assert.AreEqual(ConnectionState.Open, connection.State);
        }

        [Test]
        public void OpenConnectionTwice_WithValidConnectionString_OpensConnection()
        {
            // Arrange
            var mockConnection = new Mock<IInternalConnection>();
            var mockConnectionPoolManager = new Mock<IConnectionPoolManager>();
            mockConnectionPoolManager.Setup(x => x.Reserve(It.IsAny<string>(), It.IsAny<ConnectionParameters>()))
                .Returns(mockConnection.Object);

            var connection =
                new AseConnection(
                    "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;",
                    mockConnectionPoolManager.Object);

            // Act
            connection.Open();
            connection.Open(); // Twice should be a no-op.

            // Assert
            Assert.AreEqual(ConnectionState.Open, connection.State);
        }
    }
}
