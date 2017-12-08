using System.Threading;

namespace AdoNetCore.AseClient.Interface
{
    internal interface IInternalConnectionFactory
    {
        IInternalConnection GetNewConnection(CancellationToken token);
    }
}