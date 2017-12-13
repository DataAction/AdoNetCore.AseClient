﻿using System.Data;
using Moq;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    public class AseTransactionTests
    {
        [Test]
        public void ConstructTransaction_WithArgs_SetsProperties()
        {
            // Arrange
            var connection = new AseConnection();
            var isolationLevel = IsolationLevel.Serializable;

            // Act
            var transaction = new AseTransaction(connection, isolationLevel);

            // Assert
            Assert.AreEqual(connection, transaction.Connection);
            Assert.AreEqual(isolationLevel, transaction.IsolationLevel);
        }

        [Test]
        public void ExplicitCommit_WithValidTransaction_InteractsWithTheDbCommandCorrectly()
        {
            // Arrange
            var mockConnection = new Mock<IDbConnection>();
            var isolationLevel = IsolationLevel.Serializable;

            var mockCommandIsolationLevel = new Mock<IDbCommand>();
            var mockCommandBeginTransaction = new Mock<IDbCommand>();
            var mockCommandRollbackTransaction = new Mock<IDbCommand>();

            mockCommandIsolationLevel
                .SetupAllProperties()
                .Setup(x => x.ExecuteNonQuery())
                .Returns(0);

            mockCommandBeginTransaction
                .SetupAllProperties()
                .Setup(x => x.ExecuteNonQuery())
                .Returns(0);

            mockCommandRollbackTransaction
                .SetupAllProperties()
                .Setup(x => x.ExecuteNonQuery())
                .Returns(0);

            mockConnection
                .Setup(x => x.BeginTransaction(isolationLevel))
                .Returns(() =>
                {
                    // Simulate what AseConnection.BeginTransaction() does.
                    var t = new AseTransaction(mockConnection.Object, isolationLevel);
                    t.Begin();
                    return t;
                });

            mockConnection
                .SetupSequence(x => x.CreateCommand())
                .Returns(mockCommandIsolationLevel.Object)
                .Returns(mockCommandBeginTransaction.Object)
                .Returns(mockCommandRollbackTransaction.Object);


            // Act
            var connection = mockConnection.Object;
            var transaction = connection.BeginTransaction(isolationLevel);

            transaction.Commit(); // Explicit commit

            transaction.Dispose();

            // Assert
            mockCommandIsolationLevel.VerifySet(x => { x.CommandText = "SET TRANSACTION ISOLATION LEVEL 3"; });
            mockCommandIsolationLevel.VerifySet(x => { x.CommandType = CommandType.Text; });
            mockCommandIsolationLevel.Verify();

            mockCommandBeginTransaction.VerifySet(x => { x.CommandText = "BEGIN TRANSACTION"; });
            mockCommandBeginTransaction.VerifySet(x => { x.CommandType = CommandType.Text; });
            mockCommandBeginTransaction.VerifySet(x => { x.Transaction = transaction; });
            mockCommandBeginTransaction.Verify();

            mockCommandRollbackTransaction.VerifySet(x => { x.CommandText = "COMMIT TRANSACTION"; });
            mockCommandRollbackTransaction.VerifySet(x => { x.CommandType = CommandType.Text; });
            mockCommandRollbackTransaction.VerifySet(x => { x.Transaction = transaction; });
            mockCommandRollbackTransaction.Verify();
        }

        [Test]
        public void ExplicitRollback_WithValidTransaction_InteractsWithTheDbCommandCorrectly()
        {
            // Arrange
            var mockConnection = new Mock<IDbConnection>();
            var isolationLevel = IsolationLevel.Serializable;

            var mockCommandIsolationLevel = new Mock<IDbCommand>();
            var mockCommandBeginTransaction = new Mock<IDbCommand>();
            var mockCommandRollbackTransaction = new Mock<IDbCommand>();

            mockCommandIsolationLevel
                .SetupAllProperties()
                .Setup(x => x.ExecuteNonQuery())
                .Returns(0);

            mockCommandBeginTransaction
                .SetupAllProperties()
                .Setup(x => x.ExecuteNonQuery())
                .Returns(0);

            mockCommandRollbackTransaction
                .SetupAllProperties()
                .Setup(x => x.ExecuteNonQuery())
                .Returns(0);

            mockConnection
                .Setup(x => x.BeginTransaction(isolationLevel))
                .Returns(() =>
                {
                    // Simulate what AseConnection.BeginTransaction() does.
                    var t = new AseTransaction(mockConnection.Object, isolationLevel);
                    t.Begin();
                    return t;
                });

            mockConnection
                .SetupSequence(x => x.CreateCommand())
                .Returns(mockCommandIsolationLevel.Object)
                .Returns(mockCommandBeginTransaction.Object)
                .Returns(mockCommandRollbackTransaction.Object);


            // Act
            var connection = mockConnection.Object;
            var transaction = connection.BeginTransaction(isolationLevel);

            transaction.Rollback(); // Explicit rollback

            transaction.Dispose();

            // Assert
            mockCommandIsolationLevel.VerifySet(x => { x.CommandText = "SET TRANSACTION ISOLATION LEVEL 3"; });
            mockCommandIsolationLevel.VerifySet(x => { x.CommandType = CommandType.Text; });
            mockCommandIsolationLevel.Verify();

            mockCommandBeginTransaction.VerifySet(x => { x.CommandText = "BEGIN TRANSACTION"; });
            mockCommandBeginTransaction.VerifySet(x => { x.CommandType = CommandType.Text; });
            mockCommandBeginTransaction.VerifySet(x => { x.Transaction = transaction; });
            mockCommandBeginTransaction.Verify();

            mockCommandRollbackTransaction.VerifySet(x => { x.CommandText = "ROLLBACK TRANSACTION"; });
            mockCommandRollbackTransaction.VerifySet(x => { x.CommandType = CommandType.Text; });
            mockCommandRollbackTransaction.VerifySet(x => { x.Transaction = transaction; });
            mockCommandRollbackTransaction.Verify();
        }

        [Test]
        public void ImplicitRollback_WithValidTransaction_InteractsWithTheDbCommandCorrectly()
        {
            // Arrange
            var mockConnection = new Mock<IDbConnection>();
            var isolationLevel = IsolationLevel.Serializable;

            var mockCommandIsolationLevel = new Mock<IDbCommand>();
            var mockCommandBeginTransaction = new Mock<IDbCommand>();
            var mockCommandRollbackTransaction = new Mock<IDbCommand>();

            mockCommandIsolationLevel
                .SetupAllProperties()
                .Setup(x => x.ExecuteNonQuery())
                .Returns(0);

            mockCommandBeginTransaction
                .SetupAllProperties()
                .Setup(x => x.ExecuteNonQuery())
                .Returns(0);

            mockCommandRollbackTransaction
                .SetupAllProperties()
                .Setup(x => x.ExecuteNonQuery())
                .Returns(0);

            mockConnection
                .Setup(x => x.BeginTransaction(isolationLevel))
                .Returns(() =>
                {
                    // Simulate what AseConnection.BeginTransaction() does.
                    var t = new AseTransaction(mockConnection.Object, isolationLevel);
                    t.Begin();
                    return t;
                });

            mockConnection
                .SetupSequence(x => x.CreateCommand())
                .Returns(mockCommandIsolationLevel.Object)
                .Returns(mockCommandBeginTransaction.Object)
                .Returns(mockCommandRollbackTransaction.Object);


            // Act
            var connection = mockConnection.Object;
            var transaction = connection.BeginTransaction(isolationLevel);

            transaction.Dispose(); // Implicit rollback

            // Assert
            mockCommandIsolationLevel.VerifySet(x => { x.CommandText = "SET TRANSACTION ISOLATION LEVEL 3"; });
            mockCommandIsolationLevel.VerifySet(x => { x.CommandType = CommandType.Text; });
            mockCommandIsolationLevel.Verify();

            mockCommandBeginTransaction.VerifySet(x => { x.CommandText = "BEGIN TRANSACTION"; });
            mockCommandBeginTransaction.VerifySet(x => { x.CommandType = CommandType.Text; });
            mockCommandBeginTransaction.VerifySet(x => { x.Transaction = transaction; });
            mockCommandBeginTransaction.Verify();

            mockCommandRollbackTransaction.VerifySet(x => { x.CommandText = "ROLLBACK TRANSACTION"; });
            mockCommandRollbackTransaction.VerifySet(x => { x.CommandType = CommandType.Text; });
            mockCommandRollbackTransaction.VerifySet(x => { x.Transaction = transaction; });
            mockCommandRollbackTransaction.Verify();
        }
    }
}
