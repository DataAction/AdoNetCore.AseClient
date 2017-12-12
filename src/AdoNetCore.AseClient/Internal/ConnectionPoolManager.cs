using System;
using System.Collections.Concurrent;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal sealed class ConnectionPoolManager : IConnectionPoolManager
    {
        private static readonly ConcurrentDictionary<string, ConnectionPool> Pools = new ConcurrentDictionary<string, ConnectionPool>(StringComparer.OrdinalIgnoreCase);

        public IInternalConnection Reserve(string connectionString, ConnectionParameters parameters)
        {
            return Pools.GetOrAdd(connectionString, _ => new ConnectionPool(parameters)).Reserve();
        }

        public void Release(string connectionString, IInternalConnection connection)
        {
            if (Pools.TryGetValue(connectionString, out var pool))
            {
                pool.Release(connection);
            }
            else
            {
                throw new ArgumentException("No connection pool exists for that connection string");
            }
        }
    }
}
