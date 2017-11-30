using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Token
{
    internal class ParameterFormatToken : ParameterFormatCommonToken
    {
        public ParameterFormatToken() : base(TokenType.TDS_PARAMFMT) { }

        public static ParameterFormatToken Create(Stream stream, Encoding enc, IFormatToken previousFormatToken)
        {
            var t = new ParameterFormatToken();
            t.Read(stream, enc, previousFormatToken);
            return t;
        }
    }
}
