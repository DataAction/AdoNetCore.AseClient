using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Token
{
    internal class DoneProcToken : DoneProcCommonToken, IToken
    {
        public TokenType Type => TokenType.TDS_DONEPROC;

        public static DoneProcToken Create(Stream stream, Encoding enc, IFormatToken previous)
        {
            var t = new DoneProcToken();
            t.Read(stream, enc, previous);
            return t;
        }
    }
}