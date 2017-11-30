using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Token
{
    internal class ParameterFormat2Token : ParameterFormatCommonToken
    {
        public ParameterFormat2Token() : base(TokenType.TDS_PARAMFMT2) { }

        public static ParameterFormat2Token Create(Stream stream, Encoding enc, IFormatToken previousFormatToken)
        {
            var t = new ParameterFormat2Token();
            t.Read(stream, enc, previousFormatToken);
            return t;
        }
    }
}