using System;
using System.Collections.Generic;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Interface
{
    internal interface ISocket : IDisposable
    {
        void SendPacket(IPacket packet, DbEnvironment env);

        IEnumerable<IToken> ReceiveTokens(DbEnvironment env);

        DateTime LastActive { get; }
    }
}
