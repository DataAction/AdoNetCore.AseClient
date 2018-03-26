using System.Data.Common;

namespace AdoNetCore.AseClient.Tests.Benchmark
{
    public class CoreFxConnectionProvider : IConnectionProvider
    {
        public DbConnection GetConnection(string connectionString)
        {
#if NETCORE_OLD || NETCOREAPP2_0 || NET46 || NETSTANDARD2_0
            return new AseConnection(connectionString);
#else
            throw new NotSupportedException("The AdoNetCore AseClient only supports .NET 4+, .NET Core, and .NET Standard 2.0");
#endif
        }
    }
}