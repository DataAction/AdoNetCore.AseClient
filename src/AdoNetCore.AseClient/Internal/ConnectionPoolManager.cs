using System;
using System.Collections.Concurrent;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal static class ConnectionPoolManager
    {
        private static readonly ConcurrentDictionary<string, IConnectionPool> Pools = new ConcurrentDictionary<string, IConnectionPool>(StringComparer.OrdinalIgnoreCase);

        public static IInternalConnection Reserve(ConnectionParameters parameters)
        {
            return Pools.GetOrAdd(parameters.ConnectionString, _ => new ConnectionPool(parameters, new InternalConnectionFactory(parameters))).Reserve();
        }

        public static void Release(ConnectionParameters parameters, IInternalConnection connection)
        {
            if (Pools.TryGetValue(parameters.ConnectionString, out var pool))
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
