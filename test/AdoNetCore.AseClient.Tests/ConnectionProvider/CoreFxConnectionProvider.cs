using System.Data.Common;

namespace AdoNetCore.AseClient.Tests.ConnectionProvider
{
    public class CoreFxConnectionProvider : IConnectionProvider
    {
        public DbConnection GetConnection(string connectionString)
        {
            return new AseConnection(connectionString);
        }
    }
}
