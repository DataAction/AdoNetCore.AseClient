namespace AdoNetCore.AseClient.Interface
{
    internal interface IConnectionPoolManager
    {
        IInternalConnection Reserve(string connectionString, IConnectionParameters parameters);
        void Release(string connectionString, IInternalConnection connection);
        void ClearPool(string connectionString);
    }
}
