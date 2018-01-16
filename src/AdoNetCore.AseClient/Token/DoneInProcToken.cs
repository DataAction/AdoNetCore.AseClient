using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class DoneInProcToken : DoneProcCommonToken, IToken
    {
        public TokenType Type => TokenType.TDS_DONEINPROC;
        public static DoneInProcToken Create(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var t = new DoneInProcToken();
            t.Read(stream, env, previous);
            return t;
        }
    }
}
