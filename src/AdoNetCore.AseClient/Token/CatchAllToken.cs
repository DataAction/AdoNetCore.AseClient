using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    public class CatchAllToken : IToken
    {
        public TokenType Type { get; private set; }

        public CatchAllToken(TokenType type)
        {
            Type = type;
        }

        public void Write(Stream stream, Encoding enc)
        {

        }

        public void Read(Stream stream, Encoding enc, IToken previous)
        {
            var remainingLength = stream.ReadShort();

            for (var i = 0; i < remainingLength; i++)
            {
                stream.ReadByte();
            }
        }
    }
}
