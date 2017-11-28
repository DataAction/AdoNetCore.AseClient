using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    public class EnvironmentChangeToken : IToken
    {
        public TokenType Type => TokenType.TDS_ENVCHANGE;

        public void Write(Stream stream, Encoding enc)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, Encoding enc, IToken previous)
        {
            var remainingLength = stream.ReadShort();
        }

        public static EnvironmentChangeToken Create(Stream stream, Encoding enc, IToken previous)
        {
            var t = new EnvironmentChangeToken();
            t.Read(stream, enc, previous);
            return t;
        }
    }
}
