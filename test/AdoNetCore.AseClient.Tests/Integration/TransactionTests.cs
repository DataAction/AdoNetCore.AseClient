using System.Data;
using AdoNetCore.AseClient.Internal;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    public class TransactionTests
    {
        private IDbConnection GetConnection()
        {
            Logger.Enable();
            return new AseConnection(ConnectionStrings.Pooled);
        }

        [SetUp]
        public void Setup()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("create table [dbo].[test_foobar] (x int unique)");
            }
        }

        [Test]
        public void InsertRecord_Dispose_ImplicitRollback_ShouldWork()
        {
            using (var connection = GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                connection.Execute("insert into [dbo].[test_foobar] (x) values (1)", transaction: transaction);
            }

            using (var connection = GetConnection())
            {
                Assert.IsEmpty(connection.Query<int?>("select * from [dbo].[test_foobar]"));
            }
        }

        [Test]
        public void InsertRecord_ExplicitRollback_ShouldWork()
        {
            using (var connection = GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                connection.Execute("insert into [dbo].[test_foobar] (x) values (1)", transaction: transaction);
                transaction.Rollback();
            }

            using (var connection = GetConnection())
            {
                Assert.IsEmpty(connection.Query<int?>("select * from [dbo].[test_foobar]"));
            }
        }

        [Test]
        public void InsertRecord_ExplicitCommit_ShouldWork()
        {
            using (var connection = GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                connection.Execute("insert into [dbo].[test_foobar] (x) values (1)", transaction: transaction);
                transaction.Commit();
            }

            using (var connection = GetConnection())
            {
                Assert.AreEqual(1, connection.ExecuteScalar<int?>("select * from [dbo].[test_foobar]"));
            }
        }

        [Test]
        public void InsertRecord_CauseTransactionAbort_ShouldRollbackAndEmitError()
        {
            using (var connection = GetConnection())
            using (var transaction = connection.BeginTransaction())
            {

                connection.Execute("insert into [dbo].[test_foobar] (x) values (1)", transaction: transaction);
                try
                {
                    connection.Execute("insert into [dbo].[test_foobar] (x) values (1)", transaction: transaction);
                }
                catch (AseException) { }
            }

            using (var connection = GetConnection())
            {
                Assert.IsEmpty(connection.Query<int?>("select * from [dbo].[test_foobar]"));
            }
        }

        [TearDown]
        public void Teardown()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("drop table [dbo].[test_foobar]");
            }
        }
    }
}
