using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal class RegularSocket : ISocket
    {
        private readonly Socket _inner;
        private readonly ITokenParser _parser;

        public RegularSocket(Socket inner, ITokenParser parser)
        {
            _inner = inner;
            _parser = parser;
        }


        public void Dispose()
        {
            _inner.Dispose();
        }

        public int Send(byte[] buffer)
        {
            return _inner.Send(buffer);
        }

        public int Receive(byte[] buffer)
        {
            return _inner.Receive(buffer);
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
                packet.Write(ms, Encoding.ASCII);
                ms.Seek(0, SeekOrigin.Begin);

                while (ms.Position < ms.Length)
                {
                    //split into chunks and send over the wire
                    var buffer = new byte[env.PacketSize];
                    var template = HeaderTemplate(packet.Type);
                    Array.Copy(template, buffer, template.Length);
                    var copied = ms.Read(buffer, template.Length, buffer.Length - template.Length);
                    var chunkLength = template.Length + copied;
                    buffer[1] = (byte)(ms.Position >= ms.Length ? BufferStatus.TDS_BUFSTAT_EOM : BufferStatus.TDS_BUFSTAT_NONE); //todo: set other statuses?
                    buffer[2] = (byte)(chunkLength >> 8);
                    buffer[3] = (byte)chunkLength;

                    if (chunkLength == env.PacketSize)
                    {
                        _inner.Send(buffer);
                    }
                    else
                    {
                        var temp = new byte[chunkLength];
                        Array.Copy(buffer, temp, chunkLength);
                        _inner.Send(temp);
                    }
                }
            }
        }

        public IToken[] ReceiveTokens(DbEnvironment env)
        {
            using (var ms = new MemoryStream())
            {
                var buffer = new byte[env.PacketSize];
                var received = _inner.Receive(buffer);
                BufferType type = BufferType.TDS_BUF_NONE;
                while (received > 0)
                {
                    if (type == BufferType.TDS_BUF_NONE)
                    {
                        type = (BufferType)buffer[0];
                    }

                    if (received > env.HeaderSize)
                    {
                        ms.Write(buffer, env.HeaderSize, received - env.HeaderSize);
                    }

                    //todo: fix this, we may need to read the header to determine how many bytes left
                    if (received < env.PacketSize)
                    {
                        received = 0;
                    }
                    else
                    {
                        received = _inner.Receive(buffer);
                    }
                }

                ms.Seek(0, SeekOrigin.Begin);
                //Console.WriteLine(HexDump.Dump(ms.ToArray()));

                return _parser.Parse(ms, Encoding.ASCII);
            }
        }
    }
}
