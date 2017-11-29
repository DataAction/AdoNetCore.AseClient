using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Interface
{
    internal interface IToken
    {
        TokenType Type { get; }
        void Write(Stream stream, Encoding enc);
        void Read(Stream stream, Encoding enc, IToken previous);
    }
}
