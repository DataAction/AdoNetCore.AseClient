using System;
using System.Data;
using System.Net.Security;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;
using Moq;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    [Category("quick")]
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
        public void ConstructConnection_ConnectionString_IsSet()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";
            var connection = new AseConnection(connectionString);

            Assert.AreEqual(connectionString, connection.ConnectionString);
        }

        [Test]
        public void ConstructConnection_ConnectionString_NamedParametersIsTrue()
        {
            var connection = new AseConnection("Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;");

            Assert.AreEqual(true, connection.NamedParameters);
        }

        [Test]
        public void ConstructConnection_ConnectionString_NamedParametersCanBeModified()
        {
            var connection =
                new AseConnection(
                    "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;")
                {
                    NamedParameters = false
                };


            Assert.AreEqual(false, connection.NamedParameters);
        }

        [Test]
        public void ConstructConnection_WithoutOpening_DatabaseIsNull()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";
            var connection = new AseConnection(connectionString);

            Assert.IsNull(connection.Database);
        }

        [Test]
        public void ConstructConnection_WithoutOpening_DataSourceIsNull()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";
            var connection = new AseConnection(connectionString);

            Assert.IsNull(connection.DataSource);
        }

        [Test]
        public void ConstructConnection_WithoutOpening_ServerVersionIsNull()
        {
            var connectionString = "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";
            var connection = new AseConnection(connectionString);

            Assert.IsNull(connection.ServerVersion);
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
            var mockConnectionPoolManager = InitMockConnectionPoolManager();

            var connection =
                new AseConnection(
                    "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;",
                    mockConnectionPoolManager);

            // Act
            connection.Open();

            // Assert
            Assert.AreEqual(ConnectionState.Open, connection.State);
        }

        [Test]
        public void OpenConnection_WithValidConnectionString_TimeoutValueHasDefault()
        {
            // Arrange
            var mockConnectionPoolManager = InitMockConnectionPoolManager();

            var connection =
                new AseConnection(
                    "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;",
                    mockConnectionPoolManager);

            // Act
            connection.Open();

            // Assert
            Assert.AreEqual(15, connection.ConnectionTimeout);
        }

        [Test]
        public void OpenConnectionTwice_WithValidConnectionString_OpensConnection()
        {
            // Arrange
            var mockConnectionPoolManager = InitMockConnectionPoolManager();

            var connection =
                new AseConnection(
                    "Data Source=myASEserver;Port=5000;Database=myDataBase;Uid=myUsername;Pwd=myPassword;",
                    mockConnectionPoolManager);

            // Act
            connection.Open();
            connection.Open(); // Twice should be a no-op.

            // Assert
            Assert.AreEqual(ConnectionState.Open, connection.State);
        }

        [Test]
        public void ChangeDatabase_WhenOpen_Succeeds()
        {
            // Arrange
            var mockConnection = new Mock<IInternalConnection>();
            var mockConnectionPoolManager = new Mock<IConnectionPoolManager>();

            mockConnectionPoolManager
                .Setup(x => x.Reserve(It.IsAny<string>(), It.IsAny<ConnectionParameters>(), It.IsAny<IInfoMessageEventNotifier>(),It.IsAny<RemoteCertificateValidationCallback>()))
                .Returns(mockConnection.Object);

            mockConnection.Setup(x => x.ChangeDatabase(It.IsAny<string>()));

            var connection = new AseConnection("Data Source=myASEserver;Port=5000;Database=foo;Uid=myUsername;Pwd=myPassword;", mockConnectionPoolManager.Object);

            // Act
            connection.Open();
            connection.ChangeDatabase("bar");
            // Assert
            // No error...
        }

        [Test]
        public void ChangeDatabase_WhenNotOpen_Succeeds()
        {
            // Arrange
            var mockConnection = new Mock<IInternalConnection>();
            var mockConnectionPoolManager = new Mock<IConnectionPoolManager>();

            mockConnectionPoolManager
                .Setup(x => x.Reserve(It.IsAny<string>(), It.IsAny<ConnectionParameters>(), It.IsAny<IInfoMessageEventNotifier>(),It.IsAny<RemoteCertificateValidationCallback>()))
                .Returns(mockConnection.Object);

            mockConnection.Setup(x => x.ChangeDatabase(It.IsAny<string>()));

            var connection = new AseConnection("Data Source=myASEserver;Port=5000;Database=foo;Uid=myUsername;Pwd=myPassword;", mockConnectionPoolManager.Object);

            // Act
            Assert.Throws<InvalidOperationException>(() => { connection.ChangeDatabase("bar"); });
        }


        [TestCase(true)]
        [TestCase(false)]
        public void CreateCommand_WithNamedParameters_Propagates(bool namedParameters)
        {
            // Arrange
            var mockConnectionPoolManager = InitMockConnectionPoolManager();

            var connection =
                new AseConnection("Data Source=myASEserver;Port=5000;Database=foo;Uid=myUsername;Pwd=myPassword;",
                    mockConnectionPoolManager) {NamedParameters = namedParameters};

            // Act
            connection.Open();
            var command = connection.CreateCommand();

            // Assert
            Assert.IsNotNull(command);
            Assert.AreEqual(namedParameters, command.NamedParameters);
        }

        [Test]
        public void CreateCommand_Connection_IsSet()
        {
            // Arrange
            var mockConnectionPoolManager = InitMockConnectionPoolManager();

            var connection = new AseConnection("Data Source=myASEserver;Port=5000;Database=foo;Uid=myUsername;Pwd=myPassword;", mockConnectionPoolManager);

            // Act
            connection.Open();
            var command = connection.CreateCommand();

            // Assert
            Assert.IsNotNull(command);
            Assert.IsNotNull(command.Connection);
        }

        [Test]
        public void Open_StateChange_IsInvoked()
        {
            // Arrange
            var mockConnectionPoolManager = InitMockConnectionPoolManager();

            var eventCount = 0;
            var connection = new AseConnection("Data Source=myASEserver;Port=5000;Database=foo;Uid=myUsername;Pwd=myPassword;", mockConnectionPoolManager);
            connection.StateChange += (sender, args) => eventCount++;

            // Act
            connection.Open();
            
            // Assert
            Assert.AreEqual(2, eventCount); // 2 state changes occur while Opening a connection.
        }

        [Test]
        public void RepeatedDisposal_DoesNotThrow()
        {
            var mockConnectionPoolManager = InitMockConnectionPoolManager();

            var connection = new AseConnection("Data Source=myASEserver;Port=5000;Database=foo;Uid=myUsername;Pwd=myPassword;", mockConnectionPoolManager);

            connection.Open();
            connection.Dispose();
            connection.Dispose();
        }

        [Test]
        public void DoomedReturnsBroken() {
            var mockConnection = new Mock<IInternalConnection>();
            var mockConnectionPoolManager = new Mock<IConnectionPoolManager>();

            mockConnectionPoolManager
                .Setup(x => x.Reserve(It.IsAny<string>(), It.IsAny<ConnectionParameters>(), It.IsAny<IInfoMessageEventNotifier>(), It.IsAny<RemoteCertificateValidationCallback>()))
                .Returns(mockConnection.Object);

            mockConnection.SetupGet(x => x.IsDoomed).Returns(true);

            using (var connection = new AseConnection("Data Source=myASEserver;Port=5000;Database=foo;Uid=myUsername;Pwd=myPassword;", mockConnectionPoolManager.Object)) {
                connection.Open();
                Assert.AreEqual(ConnectionState.Broken, connection.State);
            }
        }

        private static IConnectionPoolManager InitMockConnectionPoolManager()
        {
            var mockConnection = new Mock<IInternalConnection>();
            var mockConnectionPoolManager = new Mock<IConnectionPoolManager>();

            mockConnectionPoolManager
                .Setup(x => x.Reserve(It.IsAny<string>(), It.IsAny<ConnectionParameters>(), It.IsAny<IInfoMessageEventNotifier>(),It.IsAny<RemoteCertificateValidationCallback>()))
                .Returns(mockConnection.Object);

            return mockConnectionPoolManager.Object;
        }
    }
}
