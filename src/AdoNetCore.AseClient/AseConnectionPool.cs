using AdoNetCore.AseClient.Interface;

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
}
