namespace AdoNetCore.AseClient.Interface
{
    internal interface IConnectionPool
    {
        /// <summary>
        /// Attempt to reserve an internal connection in the pool for use
        /// </summary>
        IInternalConnection Reserve();
        /// <summary>
        /// Release a used internal connection back into the pool for reuse or replacement
        /// </summary>
        void Release(IInternalConnection connection);

        /// <summary>
        /// The number of connections in the pool.
        /// </summary>
        int PoolSize { get; }

        /// <summary>
        /// The number of connections available in the pool.
        /// </summary>
        int Available { get; }
    }
}
