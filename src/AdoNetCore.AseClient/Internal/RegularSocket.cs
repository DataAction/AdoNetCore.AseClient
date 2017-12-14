using System;
using System.IO;
using System.Net.Sockets;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal class RegularSocket : ISocket
    {
        private readonly Socket _inner;
        private readonly ITokenParser _parser;

        public DateTime LastActive { get; private set; }

        public RegularSocket(Socket inner, ITokenParser parser)
        {
            _inner = inner;
            _parser = parser;
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        private byte[] HeaderTemplate(BufferType type) => new byte[]
        {
            (byte) type, 0, 0, 0,
            0, 0, 0, 0
        };

        public void SendPacket(IPacket packet, DbEnvironment env)
        {
            using (var ms = new MemoryStream())
            {
                packet.Write(ms, env.Encoding);

                ms.Seek(0, SeekOrigin.Begin);

                while (ms.Position < ms.Length)
                {
                    //split into chunks and send over the wire
                    var buffer = new byte[env.PacketSize];
                    var template = HeaderTemplate(packet.Type);
                    Array.Copy(template, buffer, template.Length);
                    var copied = ms.Read(buffer, template.Length, buffer.Length - template.Length);
                    var chunkLength = template.Length + copied;
                    buffer[1] = (byte)((ms.Position >= ms.Length ? BufferStatus.TDS_BUFSTAT_EOM : BufferStatus.TDS_BUFSTAT_NONE) | packet.Status);
                    buffer[2] = (byte)(chunkLength >> 8);
                    buffer[3] = (byte)chunkLength;

                    _inner.EnsureSend(buffer, 0, chunkLength);
                }

                LastActive = DateTime.UtcNow;
            }
        }

        public IToken[] ReceiveTokens(DbEnvironment env)
        {
            using (var ms = new MemoryStream())
            {
                bool canceled;
                var buffer = new byte[env.PacketSize];
                while (true)
                {
                    _inner.EnsureReceive(buffer, 0, env.HeaderSize);
                    var length = buffer[2] << 8 | buffer[3];
                    var bufferStatus = (BufferStatus)buffer[1];
                    _inner.EnsureReceive(buffer, env.HeaderSize, length - env.HeaderSize);
                    ms.Write(buffer, env.HeaderSize, length - env.HeaderSize);

                    //" If TDS_BUFSTAT_ATTNACK not also TDS_BUFSTAT_EOM, continue reading packets until TDS_BUFSTAT_EOM."
                    canceled = bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_ATTNACK);

                    if (bufferStatus.HasFlag(BufferStatus.TDS_BUFSTAT_EOM))
                    {
                        break;
                    }
                }

                if (canceled)
                {
                    return new IToken[0];
                }

                ms.Seek(0, SeekOrigin.Begin);

                LastActive = DateTime.UtcNow;
                return _parser.Parse(ms, env.Encoding);
            }
        }
    }
}
