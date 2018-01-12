using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class ReturnStatusToken : IToken
    {
        public TokenType Type => TokenType.TDS_RETURNSTATUS;

        public int Status { get; set; }

        public void Write(Stream stream, DbEnvironment env)
        {
            stream.WriteByte((byte)Type);
            stream.WriteInt(Status);
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            Status = stream.ReadInt();
            Logger.Instance?.WriteLine($"<- {Type}: {Status}");
        }

        public static ReturnStatusToken Create(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var t = new ReturnStatusToken();
            t.Read(stream, env, previous);
            return t;
        }
    }
}
