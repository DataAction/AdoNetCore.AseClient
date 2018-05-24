#if NET_CORE
using System;
#endif
using System.Data.Common;

namespace AdoNetCore.AseClient.Tests.ConnectionProvider
{
    public class SapConnectionProvider : IConnectionProvider
    {
        public DbConnection GetConnection(string connectionString)
        {
#if NET_FRAMEWORK
            return new Sybase.Data.AseClient.AseConnection(connectionString);
#else
            throw new NotSupportedException("The SAP AseClient only supports .NET 4+");
#endif
        }
    }
}
