using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Packet
{
    public class NormalPacket : IPacket
    {
        public IToken[] Tokens { get; set; }
        public BufferType Type => BufferType.TDS_BUF_NORMAL;

        public NormalPacket(params IToken[] tokens)
        {
            Tokens = tokens;
        }

        public void Write(Stream stream, Encoding enc)
        {
            Console.WriteLine($"Write {Type}");
            foreach (var token in Tokens)
            {
                token.Write(stream, enc);
            }
        }
    }
}
