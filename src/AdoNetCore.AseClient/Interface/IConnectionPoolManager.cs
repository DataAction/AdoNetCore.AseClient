using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Interface
{
    internal interface IConnectionPoolManager
    {
        IInternalConnection Reserve(string connectionString, ConnectionParameters parameters);
        void Release(string connectionString, IInternalConnection connection);
    }
}