using System;
using System.Data.Common;

namespace AdoNetCore.AseClient.Tests.Benchmark
{
    public class CoreFxConnectionProvider : IConnectionProvider
    {
        public DbConnection GetConnection(string connectionString)
        {
#if NET_CORE || NET_FRAMEWORK
            return new AseConnection(connectionString);
#else
            throw new NotSupportedException("The AdoNetCore AseClient only supports .NET 4+ and .NET Core");
#endif
        }
    }
}