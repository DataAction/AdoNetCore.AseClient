using System;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    public class AseExceptionTests
    {
#if NET_FRAMEWORK
        [Test]
        public void Execute_InvalidBackupServerCommand_ThrowsAseException_Sap()
        {
            using (var connection = new Sybase.Data.AseClient.AseConnection(ConnectionStrings.Pooled))
            {
                var dbName = connection.QuerySingle<string>("select db_name()");

                var ex = Assert.Throws<Sybase.Data.AseClient.AseException>(() => connection.Execute($"dump database {dbName} to '/doesnotexist/foo' with compression = '101'"));
                Assert.Greater(ex.Errors.Count, 1);
                Assert.Greater(ex.Errors[0].Severity, 10);
            }
        }
#endif

        [Test]
        public void Execute_InvalidBackupServerCommand_ThrowsAseException_CoreFx()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                var dbName = connection.QuerySingle<string>("select db_name()");

                var ex = Assert.Throws<AseException>(() => connection.Execute($"dump database {dbName} to '/doesnotexist/foo' with compression = '101'"));
                Assert.Greater(ex.Errors.Count, 1);
                Assert.Greater(ex.Errors[0].Severity, 10);
            }
        }

        [Test]
        public void Execute_Sql_ThrowsAseException()
        {
            using (var connection = new AseConnection(ConnectionStrings.Pooled))
            {
                var ex = Assert.Throws<AseException>(() => connection.QuerySingle<string>("select 1/0 as error"));
                Assert.AreEqual(ex.Errors.Count, 1);
                Assert.AreEqual(ex.Errors[0].Severity, 16);
            }
        }
    }
}
