using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
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
            
        }
    }
}
