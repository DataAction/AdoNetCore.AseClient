﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal sealed class ConnectionPoolManager : IConnectionPoolManager, IEnumerable<IConnectionPool>
    {
        private static readonly ConcurrentDictionary<string, IConnectionPool> Pools = new ConcurrentDictionary<string, IConnectionPool>(StringComparer.OrdinalIgnoreCase);

        public IInternalConnection Reserve(string connectionString, IConnectionParameters parameters, IEventNotifier eventNotifier)
        {
            return Pools.GetOrAdd(connectionString, _ => new ConnectionPool(parameters, new InternalConnectionFactory(parameters))).Reserve(eventNotifier);
        }

        public void Release(string connectionString, IInternalConnection connection)
        {
            if (connection == null)
            {
                return; //essentially, it's released :)
            }

            connection.EventNotifier = null;
            if (Pools.TryGetValue(connectionString, out var pool))
            {
                pool.Release(connection);
            }
        }

        public void ClearPool(string connectionString)
        {
            //todo: implement
        }

        /// <summary>
        /// Gets a connection by connectionString.
        /// </summary>
        public static IConnectionPool GetConnectionPool(string connectionString)
        {
            return Pools.TryGetValue(connectionString, out var result) ? result : null;
        }

        /// <summary>
        /// The number of open connections in all of the connection pools.
        /// </summary>
        public static int NumberOfOpenConnections
        {
            get
            {
                var connection = 0;

                foreach (var pool in Pools)
                {
                    connection += pool.Value.PoolSize;
                }

                return connection;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IConnectionPool> GetEnumerator()
        {
            return Pools.Values.GetEnumerator();
        }
    }
}
