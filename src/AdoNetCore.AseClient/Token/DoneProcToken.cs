using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class DoneProcToken : DoneProcCommonToken, IToken
    {
        public TokenType Type => TokenType.TDS_DONEPROC;

        public static DoneProcToken Create(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var t = new DoneProcToken();
            t.Read(stream, env, previous);
            return t;
        }
    }
}