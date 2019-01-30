using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class ParameterFormatToken : ParameterFormatCommonToken
    {
        public ParameterFormatToken() : base(TokenType.TDS_PARAMFMT) { }

        public static ParameterFormatToken Create(Stream stream, DbEnvironment env, IFormatToken previous, ref bool streamExceeded)
        {
            var t = new ParameterFormatToken();
            t.Read(stream, env, previous, ref streamExceeded);
            return t;
        }
    }
}
