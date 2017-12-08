namespace AdoNetCore.AseClient.Interface
{
    internal interface IConnectionPool
    {
        IInternalConnection Reserve();
        void Release(IInternalConnection connection);
    }
}
