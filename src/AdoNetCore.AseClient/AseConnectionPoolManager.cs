using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Manages all connection pools.
    /// </summary>
    public static class AseConnectionPoolManager
    {
        /// <summary>
        /// Retrieves the connection pool that manages the connections for the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string that identifies the pool to retrieve.</param>
        /// <returns>The connection pool that manages connectionString.</returns>
        public static AseConnectionPool GetConnectionPool(string connectionString)
        {
            var pool = ConnectionPoolManager.GetConnectionPool(connectionString);

            return pool != null ? new AseConnectionPool(pool) : null;
        }

        /// <summary>
        /// The number of open connections in all of the connection pools.
        /// </summary>
        public static int NumberOfOpenConnections => ConnectionPoolManager.NumberOfOpenConnections;
    }
}