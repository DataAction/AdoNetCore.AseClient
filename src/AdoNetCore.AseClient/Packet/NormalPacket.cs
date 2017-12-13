using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Packet
{
    internal class NormalPacket : IPacket
    {
        private readonly IEnumerable<IToken> _tokens;

        public BufferType Type => BufferType.TDS_BUF_NORMAL;

        public NormalPacket(params IToken[] tokens)
        {
            _tokens = tokens;
        }

        public NormalPacket(IEnumerable<IToken> tokens)
        {
            _tokens = tokens;
        }

        public void Write(Stream stream, Encoding enc)
        {
            Logger.Instance?.WriteLine($"Write {Type}");
            foreach (var token in _tokens)
            {
                token.Write(stream, enc);
            }
        }
    }
}
