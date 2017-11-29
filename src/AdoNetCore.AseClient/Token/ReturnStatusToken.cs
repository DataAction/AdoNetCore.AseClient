using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class ReturnStatusToken : IToken
    {
        public TokenType Type => TokenType.TDS_RETURNSTATUS;

        public int Status { get; set; }

        public void Write(Stream stream, Encoding enc)
        {
            stream.WriteByte((byte)Type);
            stream.WriteInt(Status);
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previous)
        {
            Status = stream.ReadInt();
        }

        public static ReturnStatusToken Create(Stream stream, Encoding enc, IFormatToken previous)
        {
            var t = new ReturnStatusToken();
            t.Read(stream, enc, previous);
            return t;
        }
    }
}
