using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class AnsiNullTests
    {
        [Test]
        public void ExecuteStatementEqualsNull_WithAnsiNullOn_ReturnsNoRecords()
        {
            using (var connection = new AseConnection(ConnectionStrings.AnsiNullOn))
            {
                var result = connection.Query<int?>("SELECT TOP 1 1 FROM sysobjects WHERE loginame = NULL");

                CollectionAssert.IsEmpty(result);
            }
        }

        [Test]
        public void ExecuteStatementEqualsNull_WithAnsiNullOff_ReturnsRecords()
        {
            using (var connection = new AseConnection(ConnectionStrings.AnsiNullOff))
            {
                var result = connection.Query<int?>("SELECT TOP 1 1 FROM sysobjects WHERE loginame = NULL");

                CollectionAssert.IsNotEmpty(result);
            }
        }

        [Test]
        public void ExecuteStatementIsNull_WithAnsiNullOn_ReturnsRecords()
        {
            using (var connection = new AseConnection(ConnectionStrings.AnsiNullOn))
            {
                var result = connection.Query<int?>("SELECT TOP 1 1 FROM sysobjects WHERE loginame IS NULL");

                CollectionAssert.IsNotEmpty(result);
            }
        }

        [Test]
        public void ExecuteStatementIsNull_WithAnsiNullOff_ReturnsRecords()
        {
            using (var connection = new AseConnection(ConnectionStrings.AnsiNullOff))
            {
                var result = connection.Query<int?>("SELECT TOP 1 1 FROM sysobjects WHERE loginame IS NULL");

                CollectionAssert.IsNotEmpty(result);
            }
        }
    }
}
