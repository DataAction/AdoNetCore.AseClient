using System;
using System.Data;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Connection
{
    [TestFixture]
    [Category("basic")]
    public class BrokenStateTests
    {
        private AseConnection GetConnection()
        {
            Logger.Enable();
            return new AseConnection(ConnectionStrings.Pooled);
        }

        [Test]
        public void PooledConnection_WithBrokenState_IsNotReused()
        {
            //cause the broken state by triggering an InvalidCastException
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select @p";
                    var p = command.CreateParameter();
                    p.DbType = DbType.Int16;
                    p.Value = Guid.NewGuid();
                    p.ParameterName = "@p";
                    command.Parameters.Add(p);

                    //We want to have the InvalidCastException thrown, so if the type coercion logic changes later this test can be updated
                    Assert.Throws<InvalidCastException>(() => command.ExecuteReader());
                }
                Assert.IsTrue(connection.InternalConnection.IsDoomed);
            }

            //confirm that we can open a [fresh] connection
            using (var connection = GetConnection())
            {
                connection.Open();
            }
        }
    }
}
