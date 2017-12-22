using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Manages a pool of identical connections.
    /// </summary>
    public sealed class AseConnectionPool
    {
        private readonly IConnectionPool _pool;

        internal AseConnectionPool(IConnectionPool pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// The number of connections in the pool.
        /// </summary>
        public int Size => _pool.PoolSize;

        /// <summary>
        /// The number of connections available in the pool.
        /// </summary>
        public int Available => _pool.Available;
    }

    /// <summary>
    /// Manages all connection pools.
    /// </summary>
    public sealed class AseConnectionPoolManager
    {
        /// <summary>
        /// Retrieves the connection pool that manages the connections for the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string that identifies the pool to retrieve.</param>
        /// <returns>The connection pool that manages connectionString.</returns>
        public AseConnectionPool GetConnectionPool(string connectionString)
        {
            var pool = ConnectionPoolManager.GetConnectionPool(connectionString);

            return pool != null ? new AseConnectionPool(pool) : null;
        }

        /// <summary>
        /// The number of open connections in all of the connection pools.
        /// </summary>
        public int NumberOfOpenConnections => ConnectionPoolManager.NumberOfOpenConnections;
    }
}
