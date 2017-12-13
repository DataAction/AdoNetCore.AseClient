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
    }
}
