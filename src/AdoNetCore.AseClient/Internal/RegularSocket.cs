using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal class RegularSocket : ISocket
    {
        private readonly Socket _innerSocket;
        private readonly ITokenParser _parser;
        private readonly bool _hexDump = false;

        public DateTime LastActive { get; private set; }

        public RegularSocket(Socket inner, ITokenParser parser)
        {
            _innerSocket = inner;
            _parser = parser;
        }

        public void Dispose()
        {
            _innerSocket.Dispose();
        }

        private byte[] HeaderTemplate(BufferType type) => new byte[] { (byte) type, 0, 0, 0, 0, 0, 0, 0 };
        private readonly object _sendMutex = new object();

        public void SendPacket(IPacket packet, DbEnvironment env)
        {
            lock (_sendMutex)
            {
                try
                {
                    using (var ms = new MemoryStream())
                    {
                        var bodyPacket = packet as IBodyPacket;

                        if (bodyPacket == null)
                        {
                            //eg for cancel tokens
                            var buffer = HeaderTemplate(packet.Type);
                            buffer[1] = (byte)(BufferStatus.TDS_BUFSTAT_EOM | packet.Status);
                            buffer[2] = (byte)(buffer.Length >> 8);
                            buffer[3] = (byte)buffer.Length;

                            DumpBytes(buffer);

                            _innerSocket.EnsureSend(buffer, 0, buffer.Length);
                            return;
                        }

                        bodyPacket.Write(ms, env);

                        ms.Seek(0, SeekOrigin.Begin);

                        while (ms.Position < ms.Length)
                        {
                            //split into chunks and send over the wire
                            var buffer = new byte[env.PacketSize];
                            var template = HeaderTemplate(packet.Type);
                            Buffer.BlockCopy(template, 0, buffer, 0, template.Length);
                            var copied = ms.Read(buffer, template.Length, buffer.Length - template.Length);
                            var chunkLength = template.Length + copied;
                            buffer[1] = (byte) ((ms.Position >= ms.Length ? BufferStatus.TDS_BUFSTAT_EOM : BufferStatus.TDS_BUFSTAT_NONE) | packet.Status);
                            buffer[2] = (byte) (chunkLength >> 8);
                            buffer[3] = (byte) chunkLength;

                            DumpBytes(buffer, chunkLength);

                            _innerSocket.EnsureSend(buffer, 0, chunkLength);
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
                    using (var tokenResponseStream = new TokenStream(networkStream, env))
                    {
                        foreach (var token in _parser.Parse(tokenResponseStream, env))
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

        private void DumpBytes(byte[] bytes, int length)
        {
            if (bytes.Length == length)
            {
                DumpBytes(bytes);
                return;
            }

            if (_hexDump)
            {
                var buffer = new byte[length];
                Buffer.BlockCopy(bytes, 0, buffer, 0, length);
                DumpBytes(buffer);
            }
        }

        private void DumpBytes(byte[] bytes)
        {
            if (_hexDump)
            {
                Logger.Instance?.Write(Environment.NewLine);
                Logger.Instance?.Write(HexDump.Dump(bytes));
            }
        }
    }
}
