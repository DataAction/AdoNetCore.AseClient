using System;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Interface
{
    internal interface ISocket : IDisposable
    {
        void SendPacket(IPacket packet, DbEnvironment env);

        IToken[] ReceiveTokens(DbEnvironment env);
    }
}
