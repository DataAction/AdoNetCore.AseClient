using System;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Interface
{
    internal interface ISocket : IDisposable
    {
        int Send(byte[] buffer);

        int Receive(byte[] buffer);

        void SendPacket(IPacket packet, DbEnvironment env);

        IToken[] ReceiveTokens(DbEnvironment env);
    }
}
