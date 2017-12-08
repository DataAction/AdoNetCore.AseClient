using System;
using System.Threading;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Interface
{
    internal interface ISocket : IDisposable
    {
        void SendPacket(IPacket packet, DbEnvironment env, CancellationToken? token);

        IToken[] ReceiveTokens(DbEnvironment env, CancellationToken? token);
    }
}
