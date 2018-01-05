using System.Data.Common;

namespace AdoNetCore.AseClient.Tests.Benchmark
{
    /// <summary>
    /// Abstraction to make the switching of the AseConnection possible for benchmarking the SAP and AdoNetCore implementations.
    /// </summary>
    public interface IConnectionProvider
    {
        DbConnection GetConnection(string connectionString);
    }
}