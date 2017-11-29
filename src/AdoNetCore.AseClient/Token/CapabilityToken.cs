using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class CapabilityToken : IToken
    {
        //todo: create fields to represent capabilities
        public TokenType Type => TokenType.TDS_CAPABILITY;

        public void Write(Stream stream, Encoding enc)
        {
            Console.WriteLine($"Write {Type}");
            stream.WriteByte((byte)Type);
            stream.WriteShort((short)_capabilityBytes.Length);
            stream.Write(_capabilityBytes);
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previous)
        {
            var remainingLength = stream.ReadUShort();
            var capabilityBytes = new byte[remainingLength];
            stream.Read(capabilityBytes, 0, remainingLength);
        }

        //from .net 4 client
        private readonly byte[] _capabilityBytes = {
            //cap request
            0x01, 0x0e, 0x01, 0xef, 0xff, 0x69, 0xb7, 0xfd, 0xff, 0xaf, 0x65, 0x41, 0xff, 0xff, 0xff, 0xd6,
            //cap response
            0x02, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x88, 0x40, 0x00, 0x01, 0x02, 0x48, 0x00, 0x00, 0x00
        };

        public static CapabilityToken Create(Stream stream, Encoding enc, IFormatToken previous)
        {
            var t = new CapabilityToken();
            t.Read(stream, enc, previous);
            return t;
        }
    }
}
