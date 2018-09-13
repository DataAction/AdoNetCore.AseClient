using System.Threading;
using System.Threading.Tasks;

namespace AdoNetCore.AseClient.Interface
{
    internal interface IInternalConnectionFactory
    {
        /// <summary>
        /// Create a new internal connection, ready to use.
        /// If cancellation is requested, this will discard any work done and throw an OperationCancelledException
        /// </summary>
        Task<IInternalConnection> GetNewConnection(CancellationToken token, IInfoMessageEventNotifier eventNotifier);
    }
}
