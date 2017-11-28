using System;
using System.Collections.Concurrent;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    public class ConnectionPoolManager
    {
        private static readonly ConcurrentDictionary<string, ConnectionPool> _pools = new ConcurrentDictionary<string, ConnectionPool>(StringComparer.OrdinalIgnoreCase);
        public static IInternalConnection Reserve(string connectionString)
        {
            return _pools.GetOrAdd(connectionString, _ => new ConnectionPool(connectionString)).Reserve();
        }

        public static void Release(string connectionString, IInternalConnection connection)
        {
            if (_pools.TryGetValue(connectionString, out var pool))
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
