namespace AdoNetCore.AseClient.Interface
{
    internal interface IConnectionPoolManager
    {
        IInternalConnection Reserve(string connectionString, IConnectionParameters parameters, IInfoMessageEventNotifier eventNotifier);
        void Release(string connectionString, IInternalConnection connection);
        void ClearPool(string connectionString);
        void ClearPools();
    }
}
