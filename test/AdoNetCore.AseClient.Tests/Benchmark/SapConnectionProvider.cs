using System;
using System.Data.Common;

namespace AdoNetCore.AseClient.Tests.Benchmark
{
    public class SapConnectionProvider : IConnectionProvider
    {
        public DbConnection GetConnection(string connectionString)
        {
#if NET46
            return new Sybase.Data.AseClient.AseConnection(connectionString);
#else
            throw new NotSupportedException("The SAP AseClient only supports .NET 4+");
#endif
        }
    }
}