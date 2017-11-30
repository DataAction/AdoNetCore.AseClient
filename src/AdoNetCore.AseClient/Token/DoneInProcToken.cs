using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Token
{
    internal class DoneInProcToken : DoneProcCommonToken, IToken
    {
        public TokenType Type => TokenType.TDS_DONEINPROC;
        public static DoneInProcToken Create(Stream stream, Encoding enc, IFormatToken previous)
        {
            var t = new DoneInProcToken();
            t.Read(stream, enc, previous);
            return t;
        }
    }
}
