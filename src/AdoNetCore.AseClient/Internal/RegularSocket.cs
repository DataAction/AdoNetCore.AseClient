using System;
using System.Collections.Generic;
using System.Net.Sockets;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal sealed class RegularSocket : ISocket
    {
        private readonly Socket _innerSocket;
        private readonly ITokenReader _reader;

        public DateTime LastActive { get; private set; }

        public RegularSocket(Socket inner, ITokenReader reader)
        {
            _innerSocket = inner;
            _reader = reader;
        }

        public void Dispose()
        {
            _innerSocket.Dispose();
        }

        private readonly object _sendMutex = new object();

        public void SendPacket(IPacket packet, DbEnvironment env)
        {
            lock (_sendMutex)
            {
                try
                {
                    // ReSharper disable once InconsistentlySynchronizedField
                    using (var networkStream = new NetworkStream(_innerSocket, false))
                    {
                        using (var tokenStream = new TokenStream(networkStream, env))
                        {
                            tokenStream.SetBufferType(packet.Type, packet.Status);

                            packet.Write(tokenStream, env);

                            tokenStream.Flush();
                        }
                    }
                }
                finally
                {
                    LastActive = DateTime.UtcNow;
                }
            }
        }

        public IEnumerable<IToken> ReceiveTokens(DbEnvironment env)
        {
            try
            {
                // ReSharper disable once InconsistentlySynchronizedField
                using (var networkStream = new NetworkStream(_innerSocket, false))
                {
                    using (var tokenStream = new TokenStream(networkStream, env))
                    {
                        foreach (var token in _reader.Read(tokenStream, env))
                        {
                            yield return token;
                        }
                    }
                }
            }
            finally
            {
                LastActive = DateTime.UtcNow;
            }
        }
    }
}
