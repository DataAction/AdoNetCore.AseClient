using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class DoneProcToken : DoneProcCommonToken, IToken
    {
        public TokenType Type => TokenType.TDS_DONEPROC;

        public static DoneProcToken Create(Stream stream, DbEnvironment env, IFormatToken previous, ref bool streamExceeded)
        {
            var t = new DoneProcToken();
            t.Read(stream, env, previous, ref streamExceeded);
            return t;
        }
    }
}