using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class ParameterFormat2Token : ParameterFormatCommonToken
    {
        public ParameterFormat2Token() : base(TokenType.TDS_PARAMFMT2) { }

        public static ParameterFormat2Token Create(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var t = new ParameterFormat2Token();
            t.Read(stream, env, previous);
            return t;
        }
    }
}