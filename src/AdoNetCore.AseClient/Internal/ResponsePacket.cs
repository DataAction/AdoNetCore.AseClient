using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    public class ResponsePacket : IPacket
    {
        public BufferType Type => BufferType.TDS_BUF_RESPONSE;

        public void Write(Stream stream, Encoding enc)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, Encoding enc)
        {
            var tokenType = (TokenType)stream.ReadByte();
            Console.WriteLine($"Receive token {tokenType}");
        }

        public byte[] HeaderTemplate => new byte[]
        {
            (byte)Type, 0, 0, 0,
            0, 0, 0, 0
        };
    }
}
